using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    
    #region API Controller
    [Route("api/annual-leave-planning")]
    public class AnnualLeavePlanningAPIController : FormApiControllerBase<AnnualLeavePlanningViewModel>
    {
        #region Domain Services
        public AnnualLeavePlanningService AnnualLeavePlanningService => ServiceProxy.GetService<AnnualLeavePlanningService>();
        public UserService UserService => ServiceProxy.GetService<UserService>();
        public EmployeeLeaveService EmployeeLeaveService => ServiceProxy.GetService<EmployeeLeaveService>();
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        #endregion

        #region Methods

        [HttpPost("get-reason")]
        public async Task<DataSourceResult> GetReason()
        {
            bool IsPlanning = bool.Parse("true");
            return await CoreService.GetAbsenceByPlan(IsPlanning)
                .Where(a => a.Code == "p-CutiPanjang" || a.Code == "p-CutiYearly")
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            SaveAnnualLeavePlanning(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            SaveAnnualLeavePlanning(e);
        }

        [HttpPost("save-annual-leave-planning")]
        public void SaveAnnualLeavePlanning(DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualLeavePlanningViewModel>;
            AnnualLeavePlanningService.SaveAnnualLeavePlanning(e.DocumentApprovalId, documentRequestDetail);
        }

        [HttpPost("update-annual-leave-planning")]
        public IActionResult UpdateAnnualLeavePlanning([FromBody]DocumentRequestDetailViewModel<AnnualLeavePlanningViewModel> vm)
        {
            string action = Request.Query["action"];
            AnnualLeavePlanningService.SaveAnnualLeavePlanning(vm.DocumentApprovalId, vm, action);
            return new JsonResult("Data updated successfully.");
        }

        [HttpPost("get-details/{documentApprovalId}")]
        public IEnumerable<AnnualLeavePlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            return AnnualLeavePlanningService.GetDetails(documentApprovalId);
        }

        [HttpPost("get-detail-by-id/{id}")]
        public IActionResult GetDetail(Guid id)
        {
            return new JsonResult(AnnualLeavePlanningService.GetById(id));
        }

        private EmployeeLeave GetLeave()
        {
            string noreg = User.Claims.ElementAt(0).Value;
            string noreeg = ServiceProxy.UserClaim.NoReg;
            var objLeave = EmployeeLeaveService.GetByNoreg(noreeg);

            var objLeaveOnprogress = ApprovalService.GetInprogressDraftRequestDetails("absence").Where(x => x.CreatedBy == noreeg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));
            if (objLeaveOnprogress != null)
            {
                var totalCuti = 0;
                var totalCutiPanjang = 0;

                foreach (var item in objLeaveOnprogress)
                {
                    if (item.ReasonType == "cuti")
                    {
                        totalCuti += int.Parse(item.TotalAbsence);
                    }

                    if (item.ReasonType == "cutipanjang")
                    {
                        totalCutiPanjang += int.Parse(item.TotalAbsence);
                    }
                }

                objLeave.AnnualLeave -= totalCuti;
                objLeave.LongLeave -= totalCutiPanjang;
            }

            return objLeave;
        }

        [HttpPost("upload")]
        public async Task<IEnumerable<AnnualLeavePlanningDetailView>> Upload()
        {
            string errorMessage = string.Empty;
            var uploadedExcelFile = Request.Form.Files[0];
            var mode = Request.Form["mode"];
            int yearPeriod = int.Parse(Request.Form["yearPeriod"]);
            IEnumerable<AnnualLeavePlanningDetailView> details = JsonConvert.DeserializeObject<IEnumerable<AnnualLeavePlanningDetailView>>(Request.Form["details"]);

            if(uploadedExcelFile == null || uploadedExcelFile.Length <= 0)
            {
                throw new Exception("The file cannot be empty.");
            }
            else if(!Path.GetExtension(uploadedExcelFile.FileName).Equals(".xlsx"))
            {
                throw new Exception("The uploaded file extension is not supported.");
            }
            else
            {
                return await ValidateUpload(uploadedExcelFile, yearPeriod, mode, details);
            }
        }

        private async Task<IEnumerable<AnnualLeavePlanningDetailView>> ValidateUpload(IFormFile uploadedExcelFile, int yearPeriod, string mode, IEnumerable<AnnualLeavePlanningDetailView> details)
        {
            List<AnnualLeavePlanningDetailView> result = new List<AnnualLeavePlanningDetailView>();
            using (var stream = new MemoryStream())
            {
                await uploadedExcelFile.CopyToAsync(stream);
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    var localizer = ServiceProxy.GetLocalizer();
                    string noReg = ServiceProxy.UserClaim.NoReg;//User.Claims.ElementAt(0).Value;
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                    var rowCount = workSheet.Dimension.Rows;
                    int startRow = 5;
                    int endRow = rowCount;
                    string startDateMessage = string.Empty;
                    string endDateMessage = string.Empty;
                    string reasonMessage = string.Empty;
                    string leaveMessage = string.Empty;
                    string separator = "<br/>";
                    Absence absence = new Absence();
                    List<string> errorMessages = new List<string>();
                    IEnumerable<Absence> allowedReason = CoreService.GetAbsenceByPlan(true)
                        .Where(a => a.Code == "p-CutiPanjang" || a.Code == "p-CutiYearly");
                    string dateFormat = "dd/MM/yyyy";

                    if (startRow > endRow)
                    {
                        throw new Exception("The uploaded Excel file is empty.");
                    }

                    for (int row = startRow; row <= endRow; row++)
                    {
                        DateTime startDate = DateTime.Today;
                        DateTime endDate = DateTime.Today;
                        string absenceType = string.Empty;
                        Guid absentId = Guid.Empty;
                        int days = 0;

                        startDateMessage = string.Empty;
                        endDateMessage = string.Empty;
                        reasonMessage = string.Empty;

                        if(workSheet.Cells["A" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["A" + row.ToString()].Value.ToString()))
                        {
                            startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field cannot be empty." + separator;
                        }
                        else if (!DateTime.TryParseExact(workSheet.Cells["A" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate))
                        {

                            startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value ["+ workSheet.Cells["A" + row.ToString()].Value.ToString() + "] is invalid." + separator;
                        }
                        else
                        {
                            DateTime.TryParseExact(workSheet.Cells["A" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate);

                            //if (mode.ToLower() == "edit")
                            //{
                            //    if (startDate.Year != yearPeriod || startDate.Month != DateTime.Today.AddMonths(1).Month)
                            //    {
                            //        startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value must be in " + yearPeriod.ToString() + "-" + DateTime.Today.AddMonths(1).Month.ToString().PadLeft(2, '0') + " period." + separator;
                            //    }
                            //}
                            //else
                            //{
                            //    if (startDate.Year != yearPeriod)
                            //    {
                            //        startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                            //    }
                            //}

                            if (startDate.Year != yearPeriod)
                            {
                                startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                            }
                        }

                        if (workSheet.Cells["B" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["B" + row.ToString()].Value.ToString()))
                        {
                            endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field cannot be empty." + separator;
                        } 
                        else if (!DateTime.TryParseExact(workSheet.Cells["B" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
                        {
                            endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value is invalid." + separator;
                        }
                        else
                        {
                            DateTime.TryParseExact(workSheet.Cells["B" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate);
                            if (endDate < startDate)
                            {
                                endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be less than Start Date." + separator;
                            }
                            else
                            {
                                //if(mode.ToLower() == "edit")
                                //{
                                //    if(endDate.Year != yearPeriod || endDate.Month != DateTime.Today.AddMonths(1).Month)
                                //    {
                                //        endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be in " + yearPeriod.ToString() + "-" + DateTime.Today.AddMonths(1).Month.ToString().PadLeft(2, '0') + " period." + separator;
                                //    }
                                //}
                                //else
                                //{
                                //    if (endDate.Year != yearPeriod)
                                //    {
                                //        endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                                //    }
                                //}

                                if (endDate.Year != yearPeriod)
                                {
                                    endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                                }
                            }
                        }

                        if (workSheet.Cells["C" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["C" + row.ToString()].Value.ToString()))
                        {
                            reasonMessage = reasonMessage + "Row " + row.ToString() + ": The Reason field cannot be empty." + separator;
                        }
                        else if(allowedReason.Where(r => r.Name == workSheet.Cells["C" + row.ToString()].Value.ToString()).Count() <= 0)
                        {
                            reasonMessage = reasonMessage + "Row " + row.ToString() + ": The Reason field value is invalid. Only 'Cuti Tahunan' or 'Cuti Panjang / Besar' is allowed." + separator;
                        }
                        else
                        {
                            absence = allowedReason.Where(r => r.Name == workSheet.Cells["C" + row.ToString()].Value.ToString()).FirstOrDefault();
                            if(absence != null)
                            {
                                absenceType = absence.AbsenceType;
                                absentId = absence.Id;
                            }
                        }

                        if(string.IsNullOrEmpty(startDateMessage) || string.IsNullOrEmpty(endDateMessage) || string.IsNullOrEmpty(reasonMessage))
                        {
                            // Calculate days
                            var workSchedule = TimeManagementService.GetListWorkSchEmp(noReg, startDate, endDate);
                            int maxLeaveDays = int.Parse(ConfigService.GetConfig("AnnualLeavePlanning.MaxLeaveDays").ConfigValue);
                            int offDays = workSchedule.Where(ws => ws.Off).Count();
                            int remainingAnnualLeave = 0;
                            int remainingLongLeave = 0;
                            days = (endDate - startDate).Days + 1 - offDays;

                            EmployeeLeave leave = GetLeave();
                            if (leave != null)
                            {
                                remainingAnnualLeave = leave.AnnualLeave;
                                remainingLongLeave = leave.LongLeave;
                            }

                            if (days > maxLeaveDays)
                            {
                                leaveMessage = "Row " + row.ToString() + ": Total Leave Days cannot be more than " + maxLeaveDays.ToString() + " days" + separator;
                            } 
                            else if (absence.AbsenceType == "cuti" && days > remainingAnnualLeave)
                            {
                                leaveMessage = "Row " + row.ToString() + ": Total Leave Days cannot be more than remaining Annual Leave" + separator;
                            } 
                            else if(absence.AbsenceType == "cutipanjang" && days > remainingLongLeave)
                            {
                                leaveMessage = "Row " + row.ToString() + ": Total Leave Days cannot be more than remaining Annual Leave" + separator;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(startDateMessage) || !string.IsNullOrEmpty(endDateMessage) || !string.IsNullOrEmpty(reasonMessage) || !string.IsNullOrEmpty(leaveMessage))
                        {
                            if (startDateMessage.EndsWith(separator))
                                startDateMessage = startDateMessage.Substring(0, startDateMessage.Length - separator.Length);

                            if(!string.IsNullOrEmpty(startDateMessage))
                                errorMessages.Add(startDateMessage);

                            if (endDateMessage.EndsWith(separator))
                                endDateMessage = endDateMessage.Substring(0, endDateMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(endDateMessage))
                                errorMessages.Add(endDateMessage);
                                
                            if (reasonMessage.EndsWith(separator))
                                reasonMessage = reasonMessage.Substring(0, reasonMessage.Length - separator.Length);

                            if(!string.IsNullOrEmpty(reasonMessage))
                                errorMessages.Add(reasonMessage);

                            if (leaveMessage.EndsWith(separator))
                                leaveMessage = leaveMessage.Substring(0, leaveMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(leaveMessage))
                                errorMessages.Add(leaveMessage);
                        }
                        else
                        {
                            result.Add(new AnnualLeavePlanningDetailView()
                            {
                                Id = Guid.NewGuid(),
                                NoReg = noReg,
                                StartDate = startDate,
                                EndDate = endDate,
                                Reason = workSheet.Cells["C" + row.ToString()].Value.ToString(),
                                Days = days,
                                AbsenceType = absenceType,
                                AbsentId = absentId
                            });
                        }
                    }

                    if(errorMessages.Count > 0)
                    {
                        result = new List<AnnualLeavePlanningDetailView>();
                        string finalErrorMessages = string.Empty;
                        foreach(string error in errorMessages)
                        {
                            finalErrorMessages = finalErrorMessages + error + separator;
                        }

                        throw new Exception(localizer[finalErrorMessages].Value);
                    }
                    else
                    {
                        // Validate for duplicate date
                        AnnualLeavePlanningDetailView existingData = null;

                        // Merge with form data, to make sure all data (Excel + form) is validated.
                        result.AddRange(details);
                        foreach(AnnualLeavePlanningDetailView detail in result)
                        {
                            existingData = result.Where(r => r.StartDate <= detail.StartDate && r.EndDate >= detail.EndDate && r.Id != detail.Id).FirstOrDefault();
                            if(existingData != null)
                            {
                                throw new Exception(localizer["Leave date " + existingData.StartDate.ToString("dd/MM/yyyy") + " - " + existingData.EndDate.ToString("dd/MM/yyyy") + " is already selected before."]);
                            }
                            else if (AnnualLeavePlanningService.GetActiveWorkSchedule(noReg, detail.StartDate, detail.EndDate).Count() <= 0)
                            {
                                throw new Exception(localizer["Leave date " + detail.StartDate.ToString("dd/MM/yyyy") + " - " + detail.EndDate.ToString("dd/MM/yyyy") + " must be more than 0 days."]);
                            }
                        }

                        // Validate consecutive leave date
                        int consecutiveLeaveDays = 0;
                        consecutiveLeaveDays = AnnualLeavePlanningService.CountConsecutiveLeaveDays(result);
                        Config maxLeaveDaysConfig = ConfigService.GetConfig("AnnualLeavePlanning.MaxLeaveDays");
                        int maxLeaveDays = (maxLeaveDaysConfig == null) ? 6 : int.Parse(maxLeaveDaysConfig.ConfigValue);
                        if(consecutiveLeaveDays > maxLeaveDays)
                        {
                            throw new Exception(localizer["Consecutive leave days cannot be more than " + maxLeaveDays.ToString() + " days."]);
                        }
                    }
                }
            }

            return result;
        }

        [HttpGet("download-template")]
        public IActionResult DownloadAnnualLeavePlanningTemplate(string mode, Guid docApprovalID)
        {
            using (var documentStream = GenerateAnnualLeavePlanningTemplate(mode, docApprovalID))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual Leave Planning Template", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        [HttpGet("download-summary/{documentApprovalId}")]
        public IActionResult DownloadAnnualLeavePlanningSummary(Guid documentApprovalId)
        {
            using (var documentStream = GenerateAnnualLeavePlanningSummaryExcelFile(documentApprovalId))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual Leave Planning Summary", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateAnnualLeavePlanningTemplate(string mode, Guid docApprovalID)
        {
            string noReg = User.Claims.ElementAtOrDefault(0).Value;
            string name = User.Claims.ElementAtOrDefault(2).Value;
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-leave-planning.xlsx");
            int remainingAnnualLeave = 0;
            int remainingLongLeave = 0;
            EmployeeLeave leave = GetLeave();
            if (leave != null)
            {
                remainingAnnualLeave = leave.AnnualLeave;
                remainingLongLeave = leave.LongLeave;
            }

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[0];

                    sheet.Cells["A2"].Value = noReg + " - " + name;
                    sheet.Cells["A2"].Style.Font.Bold = true;


                    sheet.Cells["A3"].Value = "Sisa Cuti Tahunan: " + remainingAnnualLeave;
                    sheet.Cells["A3"].Style.Font.Bold = true;

                    sheet.Cells["B3"].Value = "Sisa Cuti Panjang: " + remainingLongLeave;
                    sheet.Cells["B3"].Style.Font.Bold = true;

                    int row = 5;

                    //ADD DATA EXISTING IF DOWNLOAD FOR EDIT
                    //if (mode == "edit" && docApprovalID != null)
                    //{
                    //    var details = AnnualLeavePlanningService.GetDetails(docApprovalID);
                    //    // Get latest version of data
                    //    IEnumerable<AnnualLeavePlanningDetailView> currentDatas = details.Where(c => c.Version == details.Max(d => d.Version));
                    //
                    //    foreach (AnnualLeavePlanningDetailView currentData in currentDatas)
                    //    {
                    //        sheet.Cells["A" + row.ToString()].Value = currentData.StartDate.ToString("dd/MM/yyyy");
                    //        sheet.Cells["B" + row.ToString()].Value = currentData.EndDate.ToString("dd/MM/yyyy");
                    //        sheet.Cells["C" + row.ToString()].Value = currentData.Reason;
                    //
                    //        row++;
                    //    }
                    //}
                    //else
                    //{
                            sheet.Cells["A" + row.ToString()].Value = DateTime.Today.ToString("dd/MM/yyyy");
                            sheet.Cells["B" + row.ToString()].Value = DateTime.Today.ToString("dd/MM/yyyy");
                            sheet.Cells["C" + row.ToString()].Value = "Cuti Panjang / Besar";
                            sheet.Cells["D" + row.ToString()].Value = "Contoh";
                    //}

                    sheet.Column(1).Style.Numberformat.Format = "@";
                    sheet.Column(2).Style.Numberformat.Format = "@";


                    package.SaveAs(output);
                }
                return output;
            }
        }

        public MemoryStream GenerateAnnualLeavePlanningSummaryExcelFile(Guid documentApprovalId)
        {
            var data = AnnualLeavePlanningService.GetAnnualLeavePlanningSummaryExcelData(documentApprovalId);
            var details = AnnualLeavePlanningService.GetDetails(documentApprovalId);
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-leave-planning-summary.xlsx");

            MemoryStream output = new MemoryStream();
            using (FileStream documentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(documentStream))
                {
                    if(data.Count() > 0)
                    {
                        int rowStart = 3;
                        AnnualLeavePlanningSummaryStoredEntity currentPlan = data.FirstOrDefault();

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["A" + rowStart.ToString()].Value = currentPlan.NoReg;
                        sheet.Cells["B" + rowStart.ToString()].Value = currentPlan.Name;
                        sheet.Cells["C" + rowStart.ToString()].Value = currentPlan.Jan;
                        sheet.Cells["D" + rowStart.ToString()].Value = currentPlan.Feb;
                        sheet.Cells["E" + rowStart.ToString()].Value = currentPlan.Mar;
                        sheet.Cells["F" + rowStart.ToString()].Value = currentPlan.Apr;
                        sheet.Cells["G" + rowStart.ToString()].Value = currentPlan.May;
                        sheet.Cells["H" + rowStart.ToString()].Value = currentPlan.Jun;
                        sheet.Cells["I" + rowStart.ToString()].Value = currentPlan.Jul;
                        sheet.Cells["J" + rowStart.ToString()].Value = currentPlan.Aug;
                        sheet.Cells["K" + rowStart.ToString()].Value = currentPlan.Sep;
                        sheet.Cells["L" + rowStart.ToString()].Value = currentPlan.Nov;
                        sheet.Cells["M" + rowStart.ToString()].Value = currentPlan.Oct;
                        sheet.Cells["N" + rowStart.ToString()].Value = currentPlan.Dec;

                        if(data.Count() > 1)
                        {
                            AnnualLeavePlanningSummaryStoredEntity prevPlan = data.ElementAt(1);

                            if(prevPlan != null)
                            {
                                if (prevPlan.Jan != currentPlan.Jan || details.Where(d => d.StartDate.Month == 1 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["C" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["C" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Feb != currentPlan.Feb || details.Where(d => d.StartDate.Month == 2 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["D" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["D" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Mar != currentPlan.Mar || details.Where(d => d.StartDate.Month == 3 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["E" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["E" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Apr != currentPlan.Apr || details.Where(d => d.StartDate.Month == 4 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["F" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["F" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.May != currentPlan.May || details.Where(d => d.StartDate.Month == 5 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["G" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["G" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Jun != currentPlan.Jun || details.Where(d => d.StartDate.Month == 6 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["H" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["H" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Jul != currentPlan.Jul || details.Where(d => d.StartDate.Month == 7 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["I" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["I" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Aug != currentPlan.Aug || details.Where(d => d.StartDate.Month == 8 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["J" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["J" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Sep != currentPlan.Sep || details.Where(d => d.StartDate.Month == 9 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["K" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["K" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Oct != currentPlan.Oct || details.Where(d => d.StartDate.Month == 10 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["L" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["L" + rowStart.ToString()].Style.Fill.PatternColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Nov != currentPlan.Nov || details.Where(d => d.StartDate.Month == 11 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["M" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["M" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }

                                if (prevPlan.Dec != currentPlan.Dec || details.Where(d => d.StartDate.Month == 12 && d.IsUpdated).Count() > 0)
                                {
                                    sheet.Cells["N" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    sheet.Cells["N" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                }
                            }
                        }
                    }

                    package.SaveAs(output);
                }
            }
            return output;
        }

        [HttpPost("count-consecutive-leave")]
        public IActionResult CountConsecutiveLeaveDays([FromBody] ConsecutiveLeavePlanningViewModel vm)
        {
            int result = 0;

            result = AnnualLeavePlanningService.CountConsecutiveLeaveDays(vm);

            return new JsonResult(result);
        }

        [HttpGet("get-off-schedule")]
        public IActionResult GetOffSchedule()
        {
            string noreg = User.Claims.ElementAt(0).Value;
            DateTime currDate = DateTime.Today;
            DateTime nextYearDate = currDate.AddYears(1);
            DateTime startDate = new DateTime(nextYearDate.Year, 1, 1);
            DateTime endDate = new DateTime(nextYearDate.Year, 12, 31);
            var workSchedule = TimeManagementService.GetListWorkSchEmp(noreg, startDate, endDate)
                .Where(wh => wh.ShiftCode == "OFF" || wh.ShiftCode == "OFFS");
            return new JsonResult(workSchedule);
        }
        #endregion
    }
    #endregion

    #region MVC Controller

    [Area(ApplicationModule.TimeManagement)]
    [Route("annual-leave-planning")]
    public class AnnualLeavePlanningController : MvcControllerBase
    {
        #region Domain Service

        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        public AnnualLeavePlanningService AnnualLeavePlanningService => ServiceProxy.GetService<AnnualLeavePlanningService>();
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        [HttpGet("edit/{documentApprovalId}")]
        public IActionResult Edit(Guid documentApprovalId)
        {
            DocumentApproval documentApproval = ApprovalService.GetDocumentApprovalById(documentApprovalId);
            var model = new DocumentRequestDetailViewModel<AnnualLeavePlanningViewModel>();
            if(documentApproval != null)
            {
                model = ApprovalService.GetDocumentRequestDetailViewModel<AnnualLeavePlanningViewModel>(documentApprovalId, ServiceProxy.UserClaim.NoReg);
                model.FormKey = "annual-leave-planning";
            }
            else
            {
                return NotFound();
            }

            bool hasNewerVersion = AnnualLeavePlanningService.HasNewerVersion(documentApprovalId);

            ViewData.Add("documentApprovalId", documentApproval == null ? null : documentApproval.Id.ToString());
            if (!hasNewerVersion && documentApproval != null)
            {
                ViewData.Add("mode", "edit");
            }
            string noreg = User.Claims.ElementAt(0).Value;
            DateTime currDate = DateTime.Today;
            DateTime nextYearDate = currDate.AddYears(1);
            DateTime startDate = new DateTime(currDate.Year, currDate.Month, 1);
            DateTime endDate = new DateTime(nextYearDate.Year, nextYearDate.Month, 1);
            ViewBag.OffDayList = TimeManagementService.GetListWorkSchEmp(noreg, startDate, endDate)
                .Where(wh => wh.ShiftCode=="OFF" || wh.ShiftCode == "OFFS").ToList();

            return View("~/Areas/TimeManagement/Views/Form/AnnualLeavePlanning.cshtml", model);
        }
    }

    #endregion
}
