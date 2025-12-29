using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using TAMHR.ESS.Infrastructure.Validators;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    
    #region API Controller
    [Route("api/annual-wfh-planning")]
    public class AnnualWFHPlanningAPIController : FormApiControllerBase<AnnualWFHPlanningViewModel>
    {

        #region Domain Services
        public UserService UserService => ServiceProxy.GetService<UserService>();
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        protected AnnualWFHPlanningService AnnualWFHPlanningService => ServiceProxy.GetService<AnnualWFHPlanningService>();
        protected AnnualLeavePlanningService AnnualLeavePlanningService => ServiceProxy.GetService<AnnualLeavePlanningService>();
        protected AnnualPlanningService AnnualPlanningService => ServiceProxy.GetService<AnnualPlanningService>();
        #endregion

        private List<EmployeeViewModel> GetSubordinates(string noReg)
        {
            string postCode = ServiceProxy.UserClaim.PostCode;
            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noReg, postCode);
            //List<EmployeeClass7UpView> subordinates = AnnualBDJKPlanningService.GetSubordinates(noReg).ToList();
            List<EmployeeOrganizationStoredEntity> subordinates = MdmService.GetEmployeeByOrgCode(org.OrgCode, DateTime.Now.ToString("yyyy-MM-dd"))
                .Where(a => a.NoReg != noReg).ToList();

            IEnumerable<ActualReportingStructureView> reportEmps = MdmService.GetActualReportingStructuresParent(noReg, postCode);

            List<EmployeeViewModel> employees = new List<EmployeeViewModel>();

            foreach (var subordinate in subordinates)
            {
                EmployeeViewModel emp = new EmployeeViewModel();
                emp.NoReg = subordinate.NoReg;
                emp.Name = subordinate.Name;
                emp.PostName = subordinate.PostName;
                emp.AvatarURL = ConfigService.ResolveAvatar(subordinate.NoReg);
                employees.Add(emp);
            }

            foreach (ActualReportingStructureView reportEmp in reportEmps)
            {
                if (!employees.Exists(x => x.NoReg == reportEmp.NoReg))
                {
                    EmployeeViewModel emp = new EmployeeViewModel();
                    emp.NoReg = reportEmp.NoReg;
                    emp.Name = reportEmp.Name;
                    emp.PostName = reportEmp.PostName;
                    emp.AvatarURL = ConfigService.ResolveAvatar(reportEmp.NoReg);
                    employees.Add(emp);
                }
            }

            return employees;
        }

        [HttpGet("get-employee")]
        public IActionResult GetEmployee(string text)
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            if (text == null)
            {
                text = "";
            }
            var employees = GetSubordinates(noreg).Where(wh => wh.Name.ToLower().Contains(text.ToLower())).ToList();

            return new JsonResult(employees);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualWFHPlanningDetails.Count == 0) throw new Exception("Cannot create request because request is empty");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualWFHPlanningDetails.Count == 0) throw new Exception("Cannot update request because request is empty");
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        [HttpPost("save-annual-wfh-planning")]
        public void SaveAnnualWFHPlanning(DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel>;
            AnnualWFHPlanningService.UpsertAnnualWFHPlanningRequest(e.DocumentApprovalId, documentRequestDetail);
        }

        [HttpPost("update-annual-wfh-planning")]
        public IActionResult UpdateAnnualWFHPlanning([FromBody] DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel> vm)
        {
            string action = Request.Query["action"];
            AnnualWFHPlanningService.UpsertAnnualWFHPlanningRequest(vm.DocumentApprovalId, vm, action);
            return new JsonResult("Data updated successfully.");
        }
        private void Upsert(DocumentRequestDetailViewModel e)
        {
            string action = Request.Query["action"];
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel>;
            AnnualWFHPlanningService.UpsertAnnualWFHPlanningRequest(documentRequestDetail.DocumentApprovalId, documentRequestDetail, action);
        }

        [HttpPost("get-details/{documentApprovalId}")]
        public IEnumerable<AnnualWFHPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            return AnnualWFHPlanningService.GetDetails(documentApprovalId);
        }

        [HttpPost("get-wfh-planning-by-noreg/{Superior}/{NoReg}/{Period}")]
        public IEnumerable<AnnualWFHPlanningDetailView> GetWFHPlanningByNoReg(string Superior,string NoReg, int Period)
        {
            return AnnualWFHPlanningService.GetWFHPlanningByNoReg(Superior,NoReg, Period);
            
        }

        [HttpPost("upload")]
        public async Task<IEnumerable<AnnualWFHPlanningDetailView>> Upload()
        {
            string errorMessage = string.Empty;
            var uploadedExcelFile = Request.Form.Files[0];
            var mode = Request.Form["mode"];
            int yearPeriod = int.Parse(Request.Form["yearPeriod"]);
            IEnumerable<AnnualWFHPlanningDetailView> details = JsonConvert.DeserializeObject<IEnumerable<AnnualWFHPlanningDetailView>>(Request.Form["details"]);

            if (uploadedExcelFile == null || uploadedExcelFile.Length <= 0)
            {
                throw new Exception("The file cannot be empty.");
            }
            else if (!Path.GetExtension(uploadedExcelFile.FileName).Equals(".xlsx"))
            {
                throw new Exception("The uploaded file extension is not supported.");
            }
            else
            {
                return await Upload(uploadedExcelFile, yearPeriod, mode, details);
            }
        }

        private async Task<IEnumerable<AnnualWFHPlanningDetailView>> Upload(IFormFile uploadedExcelFile, int yearPeriod, string mode, IEnumerable<AnnualWFHPlanningDetailView> details)
        {
            List<AnnualWFHPlanningDetailView> result = new List<AnnualWFHPlanningDetailView>();
            using (var stream = new MemoryStream())
            {
                await uploadedExcelFile.CopyToAsync(stream);
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                    var rowCount = workSheet.Dimension.Rows;
                    int startRow = 4;
                    int endRow = rowCount;

                    string noRegMessage = string.Empty;
                    string startDateMessage = string.Empty;
                    string endDateMessage = string.Empty;
                    string separator = "<br/>";
                    List<string> errorMessages = new List<string>();
                    string dateFormat = "dd/MM/yyyy";
                    var localizer = ServiceProxy.GetLocalizer();
                    GeneralCategory wfhConfig = ConfigService.GetGeneralCategory("workplace-wfh");

                    string currentUserNoreg = ServiceProxy.UserClaim.NoReg;
                    string currentPostCode = ServiceProxy.UserClaim.PostCode;
                    ActualOrganizationStructure currentUserOrg = MdmService.GetActualOrganizationStructure(currentUserNoreg,currentPostCode);
                    List<string> subordinates = MdmService.GetEmployeeByOrgCode(currentUserOrg.OrgCode, DateTime.Now.ToString("yyyy-MM-dd")).Where(a => a.NoReg != currentUserNoreg).Select(x=>x.NoReg).ToList();
                    List<string> subordinatesByReportStructure = MdmService.GetActualReportingStructuresParent(currentUserNoreg, currentPostCode).Where(a => a.NoReg != currentUserNoreg && !subordinates.Contains(a.NoReg)).Select(x => x.NoReg).ToList();
                    foreach(var listData in subordinatesByReportStructure)
                    {
                        subordinates.Add(listData);
                    }

                    if (startRow > endRow)
                    {
                        throw new Exception("The uploaded Excel file is empty.");
                    }

                    for (int row = startRow; row <= endRow; row++)
                    {
                        string noreg = string.Empty;
                        int tempnoreg = 0;
                        DateTime startDate = DateTime.Today;
                        DateTime endDate = DateTime.Today;
                        string workplace = string.Empty;
                        int days = 0;

                        noRegMessage = string.Empty;
                        startDateMessage = string.Empty;
                        endDateMessage = string.Empty;

                        if (workSheet.Cells["A" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["A" + row.ToString()].Value.ToString()))
                        {
                            noRegMessage = noRegMessage + "Row " + row.ToString() + ": The NoReg field cannot be empty." + separator;
                        }
                        else if (!int.TryParse(workSheet.Cells["A" + row.ToString()].Value.ToString(), out tempnoreg))
                        {
                            noRegMessage = noRegMessage + "Row " + row.ToString() + ": The NoReg field value is invalid." + separator;
                        }
                        else if(!subordinates.Contains(workSheet.Cells["A" + row.ToString()].Value.ToString()))
                        {
                            noRegMessage = noRegMessage + "Row " + row.ToString() + " : The NoReg "+ workSheet.Cells["A" + row.ToString()].Value.ToString() + " is not your subordinates." + separator;
                        }
                        else 
                        {
                            noreg = workSheet.Cells["A" + row.ToString()].Value.ToString();
                        }

                        if (workSheet.Cells["C" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["C" + row.ToString()].Value.ToString()))
                        {
                            startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field cannot be empty." + separator;
                        }
                        else if (!DateTime.TryParseExact(workSheet.Cells["C" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate))
                        {
                            startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value is invalid." + separator;
                        }
                        else
                        {
                            DateTime.TryParseExact(workSheet.Cells["C" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate);

                            if (mode.ToLower() == "edit")
                            {
                                if (startDate.Year != yearPeriod || startDate.Month != DateTime.Today.AddMonths(1).Month)
                                {
                                    startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value must be in " + yearPeriod.ToString() + "-" + DateTime.Today.AddMonths(1).Month.ToString().PadLeft(2, '0') + " period." + separator;
                                }
                            }
                            else
                            {
                                if (startDate.Year != yearPeriod)
                                {
                                    startDateMessage = startDateMessage + "Row " + row.ToString() + ": The Start Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                                }
                            }
                        }

                        if (workSheet.Cells["D" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["D" + row.ToString()].Value.ToString()))
                        {
                            endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field cannot be empty." + separator;
                        }
                        else if (!DateTime.TryParseExact(workSheet.Cells["D" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
                        {
                            endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value is invalid." + separator;
                        }
                        else
                        {
                            DateTime.TryParseExact(workSheet.Cells["D" + row.ToString()].Value.ToString(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate);
                            if (endDate < startDate)
                            {
                                endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be greater than Start Date." + separator;
                            }
                            else
                            {
                                if (mode.ToLower() == "edit")
                                {
                                    if (endDate.Year != yearPeriod || endDate.Month != DateTime.Today.AddMonths(1).Month)
                                    {
                                        endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be in " + yearPeriod.ToString() + "-" + DateTime.Today.AddMonths(1).Month.ToString().PadLeft(2, '0') + " period." + separator;
                                    }
                                }
                                else
                                {
                                    if (endDate.Year != yearPeriod)
                                    {
                                        endDateMessage = endDateMessage + "Row " + row.ToString() + ": The End Date field value must be in " + yearPeriod.ToString() + " period." + separator;
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(startDateMessage) || !string.IsNullOrEmpty(endDateMessage) || !string.IsNullOrEmpty(noRegMessage))
                        {
                            if (startDateMessage.EndsWith(separator))
                                startDateMessage = startDateMessage.Substring(0, startDateMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(startDateMessage))
                                errorMessages.Add(startDateMessage);

                            if (endDateMessage.EndsWith(separator))
                                endDateMessage = endDateMessage.Substring(0, endDateMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(endDateMessage))
                                errorMessages.Add(endDateMessage);

                            if (noRegMessage.EndsWith(separator))
                                noRegMessage = noRegMessage.Substring(0, noRegMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(noRegMessage))
                                errorMessages.Add(noRegMessage);
                        }
                        else
                        {
                            var workSchedule = TimeManagementService.GetListWorkSchEmp(noreg, startDate, endDate);
                            int offDays = workSchedule.Where(ws => ws.Off).Count();
                            days = (endDate - startDate).Days + 1 - offDays;
                            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noreg);
                            result.Add(new AnnualWFHPlanningDetailView()
                            {
                                Id = Guid.NewGuid(),
                                NoReg = noreg.ToString(),
                                StartDate = startDate,
                                EndDate = endDate,
                                WorkPlace = wfhConfig.Name,
                                Days = days,
                                Name = org.Name,
                                PostName = org.PostName
                            });
                        }
                    }

                    if (errorMessages.Count > 0)
                    {
                        result = new List<AnnualWFHPlanningDetailView>();
                        string finalErrorMessages = string.Empty;
                        foreach (string error in errorMessages)
                        {
                            finalErrorMessages = finalErrorMessages + error + separator;
                        }

                        throw new Exception(finalErrorMessages);
                    }
                    else
                    {
                        // Validate for duplicate date
                        AnnualWFHPlanningDetailView existingData = null;

                        // Merge with form data, to make sure all data (Excel + form) is validated.
                        result.AddRange(details);
                        foreach (AnnualWFHPlanningDetailView detail in result)
                        {
                            existingData = result.Where(r => r.StartDate <= detail.StartDate && r.EndDate >= detail.EndDate && r.Id != detail.Id && r.NoReg == detail.NoReg).FirstOrDefault();
                            if (existingData != null)
                            {
                                throw new Exception(localizer["WFH date " + existingData.StartDate.ToString("dd/MM/yyyy") + " - " + existingData.EndDate.ToString("dd/MM/yyyy") + " is already selected before."]);
                            }
                            else if (AnnualLeavePlanningService.GetActiveWorkSchedule(detail.NoReg, detail.StartDate, detail.EndDate).Count() <= 0)
                            {
                                throw new Exception(localizer["WFH date " + detail.StartDate.ToString("dd/MM/yyyy") + " - " + detail.EndDate.ToString("dd/MM/yyyy") + " must be more than 0 days."]);
                            }
                        }
                    }
                }
            }

            return result;
        }


        [HttpGet("download-template")]
        public IActionResult DownloadAnnualWFHPlanningTemplate(string mode, Guid docApprovalID)
        {
            //ClaimBenefitService.
            using (var documentStream = GenerateAnnualWFHPlanningTemplate(mode, docApprovalID))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual WFH Planning Template", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateAnnualWFHPlanningTemplate(string mode, Guid docApprovalID)
        {
            try
            {
                string noReg = User.Claims.ElementAtOrDefault(0).Value;
                string name = User.Claims.ElementAtOrDefault(2).Value;
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-wfh-planning.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                        worksheet.Name = "Annual WFH Planning";

                        int row = 4;

                        // Set Superior NoReg and Name
                        worksheet.Cells["A2"].Value = noReg + " - " + name;

                        //ADD DATA EXISTING IF DOWNLOAD FOR EDIT
                        if (mode == "edit" && docApprovalID != null)
                        {
                            var details = AnnualWFHPlanningService.GetDetails(docApprovalID);
                            // Get latest version of data
                            IEnumerable<AnnualWFHPlanningDetailView> currentDatas = details.Where(c => c.Version == details.Max(d => d.Version));

                            foreach (AnnualWFHPlanningDetailView currentData in currentDatas)
                            {
                                worksheet.Cells["A" + row.ToString()].Value = currentData.NoReg;
                                worksheet.Cells["B" + row.ToString()].Value = currentData.Name;;
                                worksheet.Cells["C" + row.ToString()].Value = currentData.StartDate.ToString("dd/MM/yyyy");
                                worksheet.Cells["D" + row.ToString()].Value = currentData.EndDate.ToString("dd/MM/yyyy");

                                row++;
                            }
                        }
                        else
                        {
                            // Get subordinates
                            var subordinates = GetSubordinates(noReg);
                            foreach (EmployeeViewModel subordinate in subordinates)
                            {
                                worksheet.Cells["A" + row.ToString()].Value = subordinate.NoReg;
                                worksheet.Cells["B" + row.ToString()].Value = subordinate.Name;
                                worksheet.Cells["C" + row.ToString()].Value = DateTime.Today.ToString("dd/MM/yyyy");
                                worksheet.Cells["D" + row.ToString()].Value = DateTime.Today.ToString("dd/MM/yyyy");

                                row++;
                            }
                        }

                        worksheet.Column(3).Style.Numberformat.Format = "@";
                        worksheet.Column(4).Style.Numberformat.Format = "@";

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                string mes = ex.Message;
                // Log Exception
                return null;
            }
        }

        [HttpGet("download-summary/{documentApprovalId}")]
        public IActionResult DownloadAnnualWFHPlanningSummary(Guid documentApprovalId)
        {
            using (var documentStream = GenerateAnnualWFHPlanningSummaryExcelFile(documentApprovalId))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual WFH Planning Summary", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        [HttpGet("download-working-days/{periodYear}")]
        public IActionResult DownloadAnnualWFHPlanningSummary(int periodYear)
        {
            using (var documentStream = GenerateTotalWorkingDayFile(periodYear))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Total Working Days", periodYear.ToString());
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateTotalWorkingDayFile(int periodYear)
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            var data = AnnualWFHPlanningService.GetTotalWorkingDaysExcelFileData(periodYear, noreg).ToList();
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\total-working-days.xlsx");

            MemoryStream output = new MemoryStream();
            using (FileStream documentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(documentStream))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];
                    int row = 2;
                    int col = 2;
                    int totalDays = 0;
                    foreach(AnnualWFHPlanningTotalWorkingDaysStoredEntity days in data)
                    {
                        sheet.Cells[row, col].Value = days.Days.ToString();
                        col++;
                        totalDays += days.Days;
                    }
                    //total
                    sheet.Cells[row, col].Value = totalDays.ToString();
                    package.SaveAs(output);
                }
            }
            return output;
        }
        public MemoryStream GenerateAnnualWFHPlanningSummaryExcelFile(Guid documentApprovalId)
        {
            var data = AnnualWFHPlanningService.GetAnnualWFHPlanningSummaryExcelFileData(documentApprovalId).ToList();
            var details = AnnualWFHPlanningService.GetDetails(documentApprovalId);
            var header = AnnualWFHPlanningService.GetAnnualWFHPlanningByDocumentApprovalId(documentApprovalId).FirstOrDefault();
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-wfh-planning-summary.xlsx");

            MemoryStream output = new MemoryStream();
            using (FileStream documentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(documentStream))
                {
                    if (data.Count() > 0)
                    {
                        int rowStart = 3;
                        foreach(AnnualWFHPlanningSummaryExcelFileStoredEntity currentPlan in data.Where(x=>x.Version == header.Version).ToList())
                        {
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

                            if (data.Count() > 1)
                            {
                                AnnualWFHPlanningSummaryExcelFileStoredEntity prevPlan = data.Where(x=> x.NoReg == currentPlan.NoReg && x.Version == (currentPlan.Version-1)).FirstOrDefault();

                                if (prevPlan != null)
                                {
                                    if (prevPlan.Jan != currentPlan.Jan || details.Where(d => d.StartDate.Month == 1 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["C" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["C" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Feb != currentPlan.Feb || details.Where(d => d.StartDate.Month == 2 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["D" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["D" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Mar != currentPlan.Mar || details.Where(d => d.StartDate.Month == 3 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["E" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["E" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Apr != currentPlan.Apr || details.Where(d => d.StartDate.Month == 4 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["F" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["F" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.May != currentPlan.May || details.Where(d => d.StartDate.Month == 5 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["G" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["G" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Jun != currentPlan.Jun || details.Where(d => d.StartDate.Month == 6 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["H" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["H" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Jul != currentPlan.Jul || details.Where(d => d.StartDate.Month == 7 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["I" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["I" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Aug != currentPlan.Aug || details.Where(d => d.StartDate.Month == 8 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["J" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["J" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Sep != currentPlan.Sep || details.Where(d => d.StartDate.Month == 9 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["K" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["K" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Oct != currentPlan.Oct || details.Where(d => d.StartDate.Month == 10 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["L" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["L" + rowStart.ToString()].Style.Fill.PatternColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Nov != currentPlan.Nov || details.Where(d => d.StartDate.Month == 11 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["M" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["M" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Dec != currentPlan.Dec || details.Where(d => d.StartDate.Month == 12 && d.IsUpdated && d.NoReg == currentPlan.NoReg).Count() > 0)
                                    {
                                        sheet.Cells["N" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["N" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }
                                }
                            }
                            rowStart++;
                        }

                    }

                    package.SaveAs(output);
                }
            }
            return output;
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    [Route("annual-wfh-planning")]
    public class AnnualWFHPlanningController : MvcControllerBase
    {
        #region Domain Service

        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        public AnnualWFHPlanningService AnnualWFHPlanningService => ServiceProxy.GetService<AnnualWFHPlanningService>();

        #endregion
        [HttpGet("edit/{documentApprovalId}")]
        public IActionResult Edit(Guid documentApprovalId)
        {
            DocumentApproval documentApproval = ApprovalService.GetDocumentApprovalById(documentApprovalId);
            var model = new DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel>();
            if (documentApproval != null)
            {
                model = ApprovalService.GetDocumentRequestDetailViewModel<AnnualWFHPlanningViewModel>(documentApprovalId, ServiceProxy.UserClaim.NoReg);
                model.FormKey = "annual-wfh-planning";
            }
            else
            {
                return NotFound();
            }

            bool hasNewerVersion = AnnualWFHPlanningService.HasNewerVersion(documentApprovalId);

            ViewData.Add("documentApprovalId", documentApproval == null ? null : documentApproval.Id.ToString());
            if (!hasNewerVersion && documentApproval != null)
            {
                ViewData.Add("mode", "edit");
            }

            return View("~/Areas/TimeManagement/Views/Form/AnnualWFHPlanning.cshtml", model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }

    #endregion
}
