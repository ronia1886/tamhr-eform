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
    [Route("api/annual-bdjk-planning")]
    public class AnnualBDJKPlanningAPIController : FormApiControllerBase<AnnualBDJKPlanningViewModel>
    {

        #region Domain Services
        public UserService UserService => ServiceProxy.GetService<UserService>();
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        protected AnnualBDJKPlanningService AnnualBDJKPlanningService => ServiceProxy.GetService<AnnualBDJKPlanningService>();
        protected AnnualPlanningService AnnualPlanningService => ServiceProxy.GetService<AnnualPlanningService>();
        protected ConfigService configService => ServiceProxy.GetService<ConfigService>();
        #endregion

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

        [HttpPost("get-bdjk-list")]
        public List<DropDownTreeItemModel> GetBDJKList([FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            List<GeneralCategory> BDJKCodeList = configService.GetGeneralCategories("BdjkCode").ToList();

            List<string> validBDJKCode = BDJKCodeList.Select(x => x.Name.ToUpper()).ToList();

            do
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    validBDJKCode.Remove("A");
                }
                else
                {
                    validBDJKCode.Remove("B");
                    validBDJKCode.Remove("C");
                }
            }
            while ((startDate = startDate.AddDays(1)) <= endDate);

            if(validBDJKCode.Count < 3) //KALAU HANYA TERSISA D,T include A,B,C
            {
                validBDJKCode.Add("A");
                validBDJKCode.Add("B");
                validBDJKCode.Add("C");
            }

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var bdjkcode in validBDJKCode)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = bdjkcode,
                    Text = bdjkcode,
                    // Set expanded by default.
                    Expanded = false
                }) ;
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            return listDropDownTreeItem;
        }

        private List<EmployeeViewModel> GetSubordinates(string noReg)
        {
            string postCode = ServiceProxy.UserClaim.PostCode;
            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noReg, postCode);
            //List<EmployeeClass7UpView> subordinates = AnnualBDJKPlanningService.GetSubordinates(noReg).ToList();
            List<EmployeeOrganizationStoredEntity> subordinates = MdmService.GetEmployeeByOrgCode(org.OrgCode, DateTime.Now.ToString("yyyy-MM-dd"))
                .Where(a => a.NoReg != noReg && a.JobLevel != 999 && a.PositionLevel > 6).ToList();

            IEnumerable<ActualReportingStructureView> reportEmps = MdmService.GetActualReportingStructuresParent(noReg, postCode).Where(a => a.NoReg != noReg && a.PositionLevel > 6).ToList();

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

            foreach(ActualReportingStructureView reportEmp in reportEmps)
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

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualBDJKPlanningDetails.Count == 0) throw new Exception("Cannot create request because request is empty");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualBDJKPlanningDetails.Count == 0) throw new Exception("Cannot update request because request is empty");
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        [HttpPost("save-annual-bdjk-planning")]
        public void SaveAnnualBDJKPlanning(DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel>;
            AnnualBDJKPlanningService.UpsertAnnualBDJKPlanningRequest(e.DocumentApprovalId, documentRequestDetail);
        }

        [HttpPost("update-annual-bdjk-planning")]
        public IActionResult UpdateAnnualBDJKPlanning([FromBody] DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel> vm)
        {
            string action = Request.Query["action"];
            AnnualBDJKPlanningService.UpsertAnnualBDJKPlanningRequest(vm.DocumentApprovalId, vm, action);
            return new JsonResult("Data updated successfully.");
        }
        
        private void Upsert(DocumentRequestDetailViewModel e)
        {
            string action = Request.Query["action"];
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel>;
            AnnualBDJKPlanningService.UpsertAnnualBDJKPlanningRequest(documentRequestDetail.DocumentApprovalId, documentRequestDetail, action);
        }

        [HttpPost("get-details/{documentApprovalId}")]
        public IEnumerable<AnnualBDJKPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            return AnnualBDJKPlanningService.GetDetails(documentApprovalId);
        }

        [HttpPost("get-bdjk-planning-by-noreg/{Superior}/{NoReg}/{Period}")]
        public IEnumerable<AnnualBDJKPlanningDetailView> GetBDJKPlanningByNoReg(string Superior, string NoReg, int Period)
        {
            return AnnualBDJKPlanningService.GetBDJKPlanningByNoReg(Superior, NoReg, Period);

        }

        [HttpPost("upload")]
        public async Task<IEnumerable<AnnualBDJKPlanningDetailView>> Upload()
        {
            string errorMessage = string.Empty;
            var uploadedExcelFile = Request.Form.Files[0];
            var mode = Request.Form["mode"];
            int yearPeriod = int.Parse(Request.Form["yearPeriod"]);
            IEnumerable<AnnualBDJKPlanningDetailView> details = JsonConvert.DeserializeObject<IEnumerable<AnnualBDJKPlanningDetailView>>(Request.Form["details"]);

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

        private async Task<IEnumerable<AnnualBDJKPlanningDetailView>> Upload(IFormFile uploadedExcelFile, int yearPeriod, string mode, IEnumerable<AnnualBDJKPlanningDetailView> details)
        {
            List<AnnualBDJKPlanningDetailView> result = new List<AnnualBDJKPlanningDetailView>();
            using (var stream = new MemoryStream())
            {
                await uploadedExcelFile.CopyToAsync(stream);
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                    var rowCount = workSheet.Dimension.Rows;
                    int startRow = 3;
                    int endRow = rowCount;

                    string noRegMessage = string.Empty;
                    string startDateMessage = string.Empty;
                    string endDateMessage = string.Empty;
                    string bdjkCodeMessage = string.Empty;
                    string activityMessage = string.Empty;
                    string separator = "<br/>";
                    List<string> errorMessages = new List<string>();
                    string dateFormat = "dd/MM/yyyy";
                    var localizer = ServiceProxy.GetLocalizer();

                    string currentUserNoreg = ServiceProxy.UserClaim.NoReg;
                    string currentPostCode = ServiceProxy.UserClaim.PostCode;
                    ActualOrganizationStructure currentUserOrg = MdmService.GetActualOrganizationStructure(currentUserNoreg,currentPostCode);
                    List<string> subordinates = MdmService.GetEmployeeByOrgCode(currentUserOrg.OrgCode, DateTime.Now.ToString("yyyy-MM-dd")).Where(a => a.NoReg != currentUserNoreg).Select(x=>x.NoReg).ToList();
                    List<string> subordinatesByReportStructure = MdmService.GetActualReportingStructuresParent(currentUserNoreg, currentPostCode).Where(a => a.NoReg != currentUserNoreg && !subordinates.Contains(a.NoReg)).Select(x => x.NoReg).ToList();
                    foreach (var listData in subordinatesByReportStructure)
                    {
                        subordinates.Add(listData);
                    }

                    List<GeneralCategory> BDJKCodeList = configService.GetGeneralCategories("BdjkCode").ToList();
                    List<GeneralCategory> ActivityList = configService.GetGeneralCategories("CategorySPKL").ToList();
                    List<string> validBDJKCode = BDJKCodeList.Select(x => x.Name.ToUpper()).ToList();
                    List<string> validActivity = ActivityList.Select(x => x.Name.ToUpper()).ToList();

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
                        string bdjkCode = string.Empty;
                        string activity = string.Empty;
                        int days = 0;

                        noRegMessage = string.Empty;
                        startDateMessage = string.Empty;
                        endDateMessage = string.Empty;
                        bdjkCodeMessage = string.Empty;
                        activityMessage = string.Empty;

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

                        if (workSheet.Cells["E" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["E" + row.ToString()].Value.ToString()))
                        {
                            bdjkCodeMessage = bdjkCodeMessage + "Row " + row.ToString() + ": The BDJK Code field cannot be empty." + separator;
                        }
                        else if (!validBDJKCode.Contains(workSheet.Cells["E" + row.ToString()].Value.ToString().ToUpper()))
                        {
                            bdjkCodeMessage = bdjkCodeMessage + "Row " + row.ToString() + ": The BDJK Code field value is invalid." + separator;
                        }
                        else
                        {
                            bdjkCode = workSheet.Cells["E" + row.ToString()].Value.ToString().ToUpper();
                        }

                        if (workSheet.Cells["F" + row.ToString()].Value == null || string.IsNullOrEmpty(workSheet.Cells["F" + row.ToString()].Value.ToString()))
                        {
                            activityMessage = activityMessage + "Row " + row.ToString() + ": The Activity field cannot be empty." + separator;
                        }
                        else if (!validActivity.Contains(workSheet.Cells["F" + row.ToString()].Value.ToString().ToUpper()))
                        {
                            activityMessage = activityMessage + "Row " + row.ToString() + ": The Activity field value is invalid." + separator;
                        }
                        else
                        {
                            activity = workSheet.Cells["F" + row.ToString()].Value.ToString().ToUpper();
                        }

                        if (!string.IsNullOrEmpty(startDateMessage) || !string.IsNullOrEmpty(endDateMessage) || !string.IsNullOrEmpty(noRegMessage) || !string.IsNullOrEmpty(bdjkCodeMessage) || !string.IsNullOrEmpty(activityMessage))
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

                            if (bdjkCodeMessage.EndsWith(separator))
                                bdjkCodeMessage = bdjkCodeMessage.Substring(0, bdjkCodeMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(bdjkCodeMessage))
                                errorMessages.Add(bdjkCodeMessage);

                            if (activityMessage.EndsWith(separator))
                                activityMessage = activityMessage.Substring(0, activityMessage.Length - separator.Length);

                            if (!string.IsNullOrEmpty(activityMessage))
                                errorMessages.Add(activityMessage);
                        }
                        else
                        {
                            var workSchedule = TimeManagementService.GetListWorkSchEmp(noreg, startDate, endDate);
                            int offDays = workSchedule.Where(ws => ws.Off).Count();
                            days = (endDate - startDate).Days + 1 - ((bdjkCode == "C" || bdjkCode == "B") ? 0 : offDays);
                            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noreg);
                            string bdjk_code = BDJKCodeList.Where(x => x.Name.ToUpper() == bdjkCode).FirstOrDefault().Code;
                            string activity_code = ActivityList.Where(x => x.Name.ToUpper() == activity).FirstOrDefault().Code;
                            result.Add(new AnnualBDJKPlanningDetailView()
                            {
                                Id = Guid.NewGuid(),
                                NoReg = noreg.ToString(),
                                StartDate = startDate,
                                EndDate = endDate,
                                BDJKCode = bdjk_code,
                                BDJKCodeName = bdjkCode,
                                ActivityCode = activity_code,
                                Activity = activity,
                                Days = days,
                                Name = org.Name,
                                PostName = org.PostName
                            }); ;
                        }
                    }

                    if (errorMessages.Count > 0)
                    {
                        result = new List<AnnualBDJKPlanningDetailView>();
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
                        AnnualBDJKPlanningDetailView existingData = null;

                        // Merge with form data, to make sure all data (Excel + form) is validated.
                        result.AddRange(details);
                        foreach (AnnualBDJKPlanningDetailView detail in result)
                        {
                            existingData = result.Where(r => r.StartDate <= detail.StartDate && r.EndDate >= detail.EndDate && r.Id != detail.Id && r.NoReg == detail.NoReg).FirstOrDefault();
                            if (existingData != null)
                            {
                                throw new Exception(localizer["BDJK date " + existingData.StartDate.ToString("dd/MM/yyyy") + " - " + existingData.EndDate.ToString("dd/MM/yyyy") + " is already selected before."]);
                            }
                        }
                    }
                }
            }

            return result;
        }

        [HttpGet("download-template")]
        public IActionResult DownloadAnnualBDJKPlanningTemplate(string mode, Guid docApprovalID)
        {
            //ClaimBenefitService.
            using (var documentStream = GenerateAnnualBDJKPlanningTemplate(mode, docApprovalID))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual BDJK Planning Template", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateAnnualBDJKPlanningTemplate(string mode, Guid docApprovalID)
        {
            try
            {
                string noReg = User.Claims.ElementAtOrDefault(0).Value;
                string nooreg = ServiceProxy.UserClaim.NoReg;
                string name = User.Claims.ElementAtOrDefault(2).Value;
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-bdjk-planning.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        worksheet.Name = "Annual BDJK Planning";

                        int row = 3;

                        //ADD DATA EXISTING IF DOWNLOAD FOR EDIT
                        if (mode == "edit" && docApprovalID != null)
                        {
                            var details = AnnualBDJKPlanningService.GetDetails(docApprovalID);
                            // Get latest version of data
                            IEnumerable<AnnualBDJKPlanningDetailView> currentDatas = details.Where(c => c.Version == details.Max(d => d.Version));

                            foreach (AnnualBDJKPlanningDetailView currentData in currentDatas)
                            {
                                worksheet.Cells["A" + row.ToString()].Value = currentData.NoReg;
                                worksheet.Cells["B" + row.ToString()].Value = currentData.Name;
                                worksheet.Cells["C" + row.ToString()].Value = currentData.StartDate.ToString("dd/MM/yyyy");
                                worksheet.Cells["D" + row.ToString()].Value = currentData.EndDate.ToString("dd/MM/yyyy");

                                row++;
                            }
                        }
                        else
                        {
                            // Get subordinates
                            var subordinates = GetSubordinates(nooreg);
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
        public IActionResult DownloadAnnualLeavePlanningSummary(Guid documentApprovalId)
        {
            using (var documentStream = GenerateAnnualBDJKPlanningSummaryExcelFile(documentApprovalId))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual BDJK Planning Summary", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateAnnualBDJKPlanningSummaryExcelFile(Guid documentApprovalId)
        {
            var data = AnnualBDJKPlanningService.GetAnnualBDJKPlanningSummaryExcelData(documentApprovalId);
            var details = AnnualBDJKPlanningService.GetDetails(documentApprovalId);
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-bdjk-planning-summary.xlsx");

            MemoryStream output = new MemoryStream();
            using (FileStream documentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(documentStream))
                {
                    if (data.Count() > 0)
                    {
                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];
                        int rowStart = 3;

                        // Get latest version of data
                        IEnumerable<AnnualBDJKPlanningSummaryStoredEntity> currentPlans = data.Where(c => c.Version == data.Max(d => d.Version));

                        foreach(AnnualBDJKPlanningSummaryStoredEntity currentPlan in currentPlans)
                        {
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
                                AnnualBDJKPlanningSummaryStoredEntity prevPlan = data.Where(a => a.NoReg == currentPlan.NoReg && a.Version == currentPlan.Version - 1).FirstOrDefault();

                                if (prevPlan != null)
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
    [Route("annual-bdjk-planning")]
    public class AnnualbdjkPlanningController : MvcControllerBase
    {
        #region Domain Service

        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        public AnnualBDJKPlanningService AnnualBDJKPlanningService => ServiceProxy.GetService<AnnualBDJKPlanningService>();

        #endregion
        [HttpGet("edit/{documentApprovalId}")]
        public IActionResult Edit(Guid documentApprovalId)
        {
            DocumentApproval documentApproval = ApprovalService.GetDocumentApprovalById(documentApprovalId);
            var model = new DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel>();
            if (documentApproval != null)
            {
                model = ApprovalService.GetDocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel>(documentApprovalId, ServiceProxy.UserClaim.NoReg);
                model.FormKey = "annual-bdjk-planning";
            }
            else
            {
                return NotFound();
            }

            bool hasNewerVersion = AnnualBDJKPlanningService.HasNewerVersion(documentApprovalId);

            ViewData.Add("documentApprovalId", documentApproval == null ? null : documentApproval.Id.ToString());
            if (!hasNewerVersion && documentApproval != null)
            {
                ViewData.Add("mode", "edit");
            }

            return View("~/Areas/TimeManagement/Views/Form/AnnualBDJKPlanning.cshtml", model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }

    #endregion
}
