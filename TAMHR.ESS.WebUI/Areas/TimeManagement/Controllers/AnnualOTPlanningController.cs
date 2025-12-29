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
    [Route("api/annual-ot-planning")]
    public class AnnualOTPlanningAPIController : FormApiControllerBase<AnnualOTPlanningViewModel>
    {

        #region Domain Services
        public UserService UserService => ServiceProxy.GetService<UserService>();
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        protected AnnualOTPlanningService AnnualOTPlanningService => ServiceProxy.GetService<AnnualOTPlanningService>();
        #endregion

        [HttpPost("get-employee")]
        public async Task<DataSourceResult> GetEmployee()
        {
            bool IsPlanning = bool.Parse("true");
            string noreg = ServiceProxy.UserClaim.NoReg;
            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noreg);
            List<EmployeeOrganizationStoredEntity> underlings = MdmService.GetEmployeeByOrgCode(org.OrgCode, DateTime.Now.ToString("yyyy-MM-dd")).Where(a => a.NoReg != noreg).ToList();
            List<EmployeeViewModel> employees = new List<EmployeeViewModel>();
            foreach(EmployeeOrganizationStoredEntity underling in underlings)
            {
                EmployeeViewModel emp = new EmployeeViewModel();
                emp.NoReg = underling.NoReg;
                emp.Name = underling.Name;
                emp.PostName = underling.PostName;
                emp.AvatarURL = ConfigService.ResolveAvatar(underling.NoReg);
                employees.Add(emp);
            }

            return await employees.ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-workplace")]
        public async Task<DataSourceResult> GetWorkPlace()
        {
            return await ConfigService.GetConfigs()
                .Where(a => a.ConfigKey == "OT.WorkPlace")
                .ToDataSourceResultAsync(new DataSourceRequest());
        }
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<AnnualOTPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualOTPlanningDetails.Count == 0) throw new Exception("Cannot create request because request is empty");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<AnnualOTPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.AnnualOTPlanningDetails.Count == 0) throw new Exception("Cannot update request because request is empty");
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        [HttpPost("save-annual-ot-planning")]
        public void SaveAnnualOTPlanning(DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualOTPlanningViewModel>;
            AnnualOTPlanningService.UpsertAnnualOTPlanningRequest(e.DocumentApprovalId, documentRequestDetail);
        }

        [HttpPost("update-annual-ot-planning")]
        public IActionResult UpdateAnnualOTPlanning([FromBody] DocumentRequestDetailViewModel<AnnualOTPlanningViewModel> vm)
        {
            string action = Request.Query["action"];
            AnnualOTPlanningService.UpsertAnnualOTPlanningRequest(vm.DocumentApprovalId, vm, action);
            return new JsonResult("Data updated successfully.");
        }
        private void Upsert(DocumentRequestDetailViewModel e)
        {
            string action = Request.Query["action"];
            var documentRequestDetail = e as DocumentRequestDetailViewModel<AnnualOTPlanningViewModel>;
            AnnualOTPlanningService.UpsertAnnualOTPlanningRequest(documentRequestDetail.DocumentApprovalId, documentRequestDetail, action);
        }

        [HttpPost("get-details/{documentApprovalId}")]
        public IEnumerable<AnnualOTPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            return AnnualOTPlanningService.GetDetails(documentApprovalId);
        }

        [HttpPost("get-initial/{periodYear}")]
        public IEnumerable<AnnualOTPlanningDetailView> GetInitial(int periodYear)
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            string PostCode = ServiceProxy.UserClaim.PostCode;
            return AnnualOTPlanningService.GetInitial(noreg, PostCode, periodYear);
        }

        [HttpPost("upload")]
        public async Task<IEnumerable<AnnualOTPlanningDetailView>> Upload()
        {
            string errorMessage = string.Empty;
            var uploadedExcelFile = Request.Form.Files[0];
            string mode = Request.Form["mode"];
            int periodYear = Int32.Parse(Request.Form["periodYear"]);
            IEnumerable<AnnualOTPlanningDetailView> details = JsonConvert.DeserializeObject<IEnumerable<AnnualOTPlanningDetailView>>(Request.Form["details"]);

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
                return await Upload(uploadedExcelFile, mode, periodYear, details);
            }
        }

        private string ValidateMonthlyOTData(string value, int currentRow, int remarkRow, string monthName, string separator)
        {
            string errorMessage = string.Empty;
            double temp = 0;

            if ((value == null || string.IsNullOrEmpty(value)) && currentRow != remarkRow)
            {
                errorMessage = errorMessage + "Row " + currentRow.ToString() + ": The "+ monthName +" field cannot be empty." + separator;
            }
            else if (!double.TryParse(value == null ? "" : value, out temp) && currentRow != remarkRow)
            {
                errorMessage = errorMessage + "Row " + currentRow.ToString() + ": The "+ monthName +" field value is invalid." + separator;
            }

            return errorMessage;
        }

        private async Task<IEnumerable<AnnualOTPlanningDetailView>> Upload(IFormFile uploadedExcelFile, string mode, int periodYear, IEnumerable<AnnualOTPlanningDetailView> details)
        {
            List<AnnualOTPlanningDetailView> result = new List<AnnualOTPlanningDetailView>();
            using (var stream = new MemoryStream())
            {
                await uploadedExcelFile.CopyToAsync(stream);
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                    var rowCount = workSheet.Dimension.Rows;
                    int startRow = 4;
                    int remarkRow = 0;
                    int endRow = rowCount;
                    var localizer = ServiceProxy.GetLocalizer();

                    string janMessage = string.Empty;
                    string febMessage = string.Empty;
                    string marMessage = string.Empty;
                    string aprMessage = string.Empty;
                    string mayMessage = string.Empty;
                    string junMessage = string.Empty;
                    string julMessage = string.Empty;
                    string augMessage = string.Empty;
                    string sepMessage = string.Empty;
                    string octMessage = string.Empty;
                    string novMessage = string.Empty;
                    string decMessage = string.Empty;

                    string separator = "<br/>";
                    List<string> errorMessages = new List<string>();

                    string currentUserNoreg = ServiceProxy.UserClaim.NoReg;
                    List<GeneralCategory> categories = new List<GeneralCategory>();
                    string division = GetDivisionLabourType(currentUserNoreg);
                    if (division == "DL")
                    {
                        categories = ConfigService.GetGeneralCategories("ANNUALOTPLANNING-DL").ToList();
                        remarkRow = 7;
                    }
                    else
                    {
                        categories = ConfigService.GetGeneralCategories("ANNUALOTPLANNING-IDL").ToList();
                        remarkRow = 5;
                    }

                    if (startRow > endRow)
                    {
                        throw new Exception("The uploaded Excel file is empty.");
                    }

                    AnnualOTPlanningDetailView currentData = null;
                    for (int row = startRow; row <= endRow; row++)
                    {
                        string jan = string.Empty, feb = string.Empty, mar = string.Empty, apr = string.Empty, may = string.Empty, jun = string.Empty, jul = string.Empty, aug = string.Empty, sep = string.Empty, oct = string.Empty, nov = string.Empty, dec = string.Empty;
                        janMessage = string.Empty;
                        febMessage = string.Empty;
                        marMessage = string.Empty;
                        aprMessage = string.Empty;
                        mayMessage = string.Empty;
                        junMessage = string.Empty;
                        julMessage = string.Empty;
                        augMessage = string.Empty;
                        sepMessage = string.Empty;
                        octMessage = string.Empty;
                        novMessage = string.Empty;
                        decMessage = string.Empty;

                        #region check each month column
                        janMessage = ValidateMonthlyOTData(workSheet.Cells["B" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["B" + row.ToString()].Value.ToString(), row, remarkRow, "Jan", separator);
                        febMessage = ValidateMonthlyOTData(workSheet.Cells["C" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["C" + row.ToString()].Value.ToString(), row, remarkRow, "Feb", separator);
                        marMessage = ValidateMonthlyOTData(workSheet.Cells["D" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["D" + row.ToString()].Value.ToString(), row, remarkRow, "Mar", separator);
                        aprMessage = ValidateMonthlyOTData(workSheet.Cells["E" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["E" + row.ToString()].Value.ToString(), row, remarkRow, "Apr", separator);
                        mayMessage = ValidateMonthlyOTData(workSheet.Cells["F" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["F" + row.ToString()].Value.ToString(), row, remarkRow, "May", separator);
                        junMessage = ValidateMonthlyOTData(workSheet.Cells["G" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["G" + row.ToString()].Value.ToString(), row, remarkRow, "Jun", separator);
                        julMessage = ValidateMonthlyOTData(workSheet.Cells["H" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["H" + row.ToString()].Value.ToString(), row, remarkRow, "Jul", separator);
                        augMessage = ValidateMonthlyOTData(workSheet.Cells["I" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["I" + row.ToString()].Value.ToString(), row, remarkRow, "Aug", separator);
                        sepMessage = ValidateMonthlyOTData(workSheet.Cells["J" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["J" + row.ToString()].Value.ToString(), row, remarkRow, "Sep", separator);
                        octMessage = ValidateMonthlyOTData(workSheet.Cells["K" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["K" + row.ToString()].Value.ToString(), row, remarkRow, "Oct", separator);
                        novMessage = ValidateMonthlyOTData(workSheet.Cells["L" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["L" + row.ToString()].Value.ToString(), row, remarkRow, "Nov", separator);
                        decMessage = ValidateMonthlyOTData(workSheet.Cells["M" + row.ToString()].Value == null ? string.Empty : workSheet.Cells["M" + row.ToString()].Value.ToString(), row, remarkRow, "Dec", separator);

                        if(string.IsNullOrEmpty(janMessage))
                            jan = workSheet.Cells["B" + row.ToString()].Value == null ? "" : workSheet.Cells["B" + row.ToString()].Value.ToString();

                        if(string.IsNullOrEmpty(febMessage))
                            feb = workSheet.Cells["C" + row.ToString()].Value == null ? "" : workSheet.Cells["C" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(marMessage))
                            mar = workSheet.Cells["D" + row.ToString()].Value == null ? "" : workSheet.Cells["D" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(aprMessage))
                            apr = workSheet.Cells["E" + row.ToString()].Value == null ? "" : workSheet.Cells["E" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(mayMessage))
                            may = workSheet.Cells["F" + row.ToString()].Value == null ? "" : workSheet.Cells["F" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(junMessage))
                            jun = workSheet.Cells["G" + row.ToString()].Value == null ? "" : workSheet.Cells["G" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(julMessage))
                            jul = workSheet.Cells["H" + row.ToString()].Value == null ? "" : workSheet.Cells["H" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(augMessage))
                            aug = workSheet.Cells["I" + row.ToString()].Value == null ? "" : workSheet.Cells["I" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(sepMessage))
                            sep = workSheet.Cells["J" + row.ToString()].Value == null ? "" : workSheet.Cells["J" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(octMessage))
                            oct = workSheet.Cells["K" + row.ToString()].Value == null ? "" : workSheet.Cells["K" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(novMessage))
                            nov = workSheet.Cells["L" + row.ToString()].Value == null ? "" : workSheet.Cells["L" + row.ToString()].Value.ToString();

                        if (string.IsNullOrEmpty(decMessage))
                            dec = workSheet.Cells["M" + row.ToString()].Value == null ? "" : workSheet.Cells["M" + row.ToString()].Value.ToString();

                        #endregion
                        if (!string.IsNullOrEmpty(janMessage) || !string.IsNullOrEmpty(febMessage) || !string.IsNullOrEmpty(marMessage) || 
                            !string.IsNullOrEmpty(aprMessage) || !string.IsNullOrEmpty(mayMessage) || !string.IsNullOrEmpty(junMessage) || 
                            !string.IsNullOrEmpty(julMessage) || !string.IsNullOrEmpty(augMessage) || !string.IsNullOrEmpty(sepMessage) || 
                            !string.IsNullOrEmpty(octMessage) || !string.IsNullOrEmpty(novMessage) || !string.IsNullOrEmpty(decMessage))
                        {
                            if (janMessage.EndsWith(separator))
                                janMessage = janMessage.Substring(0, janMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(janMessage))
                                errorMessages.Add(janMessage);

                            if (febMessage.EndsWith(separator))
                                febMessage = febMessage.Substring(0, febMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(febMessage))
                                errorMessages.Add(febMessage);

                            if (marMessage.EndsWith(separator))
                                marMessage = marMessage.Substring(0, marMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(marMessage))
                                errorMessages.Add(marMessage);

                            if (aprMessage.EndsWith(separator))
                                aprMessage = aprMessage.Substring(0, aprMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(aprMessage))
                                errorMessages.Add(aprMessage);

                            if (mayMessage.EndsWith(separator))
                                mayMessage = mayMessage.Substring(0, mayMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(mayMessage))
                                errorMessages.Add(mayMessage);

                            if (junMessage.EndsWith(separator))
                                junMessage = junMessage.Substring(0, junMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(junMessage))
                                errorMessages.Add(junMessage);

                            if (julMessage.EndsWith(separator))
                                julMessage = julMessage.Substring(0, julMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(julMessage))
                                errorMessages.Add(julMessage);

                            if (augMessage.EndsWith(separator))
                                augMessage = augMessage.Substring(0, augMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(augMessage))
                                errorMessages.Add(augMessage);

                            if (sepMessage.EndsWith(separator))
                                sepMessage = sepMessage.Substring(0, sepMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(sepMessage))
                                errorMessages.Add(sepMessage);

                            if (octMessage.EndsWith(separator))
                                octMessage = octMessage.Substring(0, octMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(octMessage))
                                errorMessages.Add(octMessage);

                            if (novMessage.EndsWith(separator))
                                novMessage = novMessage.Substring(0, novMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(novMessage))
                                errorMessages.Add(novMessage);

                            if (decMessage.EndsWith(separator))
                                decMessage = decMessage.Substring(0, decMessage.Length - separator.Length);
                            if (!string.IsNullOrEmpty(decMessage))
                                errorMessages.Add(decMessage);
                        }
                        else
                        {
                            GeneralCategory category = new GeneralCategory();
                            string labourtype = string.Empty;
                            if(division == "DL")
                            {
                                if (row == 4)
                                {
                                    category = categories.Where(x => x.Code.ToLower().Contains("targetvolume")).FirstOrDefault();
                                    labourtype = "-";
                                }
                                if (row == 5 || row == 6)
                                {
                                    category = categories.Where(x => x.Code.ToLower().Contains("overtimeplanning")).FirstOrDefault();
                                    labourtype = row == 5 ? "DL" : "IDL";
                                }
                                if (row == 7)
                                {
                                    category = categories.Where(x => x.Code.ToLower().Contains("remark")).FirstOrDefault();
                                    labourtype = "-";
                                }
                                if (row == 8 || row == 9)
                                {
                                    category = categories.Where(x => x.Code.ToLower().Contains("otindex")).FirstOrDefault();
                                    labourtype = row == 8 ? "DL" : "IDL";
                                }
                            }
                            else
                            {
                                labourtype = "IDL";
                                if(row == 4)
                                    category = categories.Where(x => x.Code.ToLower().Contains("overtimeplanning")).FirstOrDefault();
                                if(row == 5)
                                    category = categories.Where(x => x.Code.ToLower().Contains("remark")).FirstOrDefault();
                            }

                            if(mode == "edit" && row >= startRow)
                            {
                                currentData = details.Where(d => d.CategoryCode == category.Code && d.LabourType == labourtype).FirstOrDefault();
                                if(currentData != null)
                                {
                                    // Only validate the data if the data is edited for next month
                                    if (!(((1 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["B" + row.ToString()].Value != null && workSheet.Cells["B" + row.ToString()].Value.ToString() != currentData.Jan)
                                            throw new Exception(localizer["Row " + row.ToString() + " Jan: Only the next month period could be edited."]);
                                    }

                                    if (!(( (2 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["C" + row.ToString()].Value != null && workSheet.Cells["C" + row.ToString()].Value.ToString() != currentData.Feb)
                                            throw new Exception(localizer["Row " + row.ToString() + " Feb: Only the next month period could be edited."]);
                                    }

                                    if ((((3 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["D" + row.ToString()].Value != null && workSheet.Cells["D" + row.ToString()].Value.ToString() != currentData.Mar)
                                            throw new Exception(localizer["Row " + row.ToString() + " Mar: Only the next month period could be edited."]);
                                    }

                                    if ((((4 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["E" + row.ToString()].Value != null && workSheet.Cells["E" + row.ToString()].Value.ToString() != currentData.Apr)
                                            throw new Exception(localizer["Row " + row.ToString() + " Apr: Only the next month period could be edited."]);
                                    }

                                    if ((((5 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["F" + row.ToString()].Value != null && workSheet.Cells["F" + row.ToString()].Value.ToString() != currentData.May)
                                            throw new Exception(localizer["Row " + row.ToString() + " May: Only the next month period could be edited."]);
                                    }

                                    if ((((6 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["G" + row.ToString()].Value != null && workSheet.Cells["G" + row.ToString()].Value.ToString() != currentData.Jun)
                                            throw new Exception(localizer["Row " + row.ToString() + " Jun: Only the next month period could be edited."]);
                                    }

                                    if ((((7 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["H" + row.ToString()].Value != null && workSheet.Cells["H" + row.ToString()].Value.ToString() != currentData.Jul)
                                            throw new Exception(localizer["Row " + row.ToString() + " Jul: Only the next month period could be edited."]);
                                    }

                                    if ((((8 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["I" + row.ToString()].Value != null && workSheet.Cells["I" + row.ToString()].Value.ToString() != currentData.Aug)
                                            throw new Exception(localizer["Row " + row.ToString() + " Aug: Only the next month period could be edited."]);
                                    }

                                    if ((((9 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["J" + row.ToString()].Value != null && workSheet.Cells["J" + row.ToString()].Value.ToString() != currentData.Sep)
                                            throw new Exception(localizer["Row " + row.ToString() + " Sep: Only the next month period could be edited."]);
                                    }

                                    if ((((10 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["K" + row.ToString()].Value != null && workSheet.Cells["K" + row.ToString()].Value.ToString() != currentData.Oct)
                                            throw new Exception(localizer["Row " + row.ToString() + " Oct: Only the next month period could be edited."]);
                                    }

                                    if ((((11 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["L" + row.ToString()].Value != null && workSheet.Cells["L" + row.ToString()].Value.ToString() != currentData.Nov)
                                            throw new Exception(localizer["Row " + row.ToString() + " Nov: Only the next month period could be edited."]);
                                    }

                                    if ((((12 - DateTime.Today.Month) + 12 * (periodYear - DateTime.Today.Year)) == 1))
                                    {
                                        if (workSheet.Cells["M" + row.ToString()].Value != null && workSheet.Cells["M" + row.ToString()].Value.ToString() != currentData.Dec)
                                            throw new Exception(localizer["Row " + row.ToString() + " Dec: Only the next month period could be edited."]);
                                    }
                                }
                            }
                            
                            result.Add(new AnnualOTPlanningDetailView()
                            {
                                Division = division,
                                CategoryCode = category.Code,
                                Category = category.Name,
                                LabourType = labourtype,
                                OrderSequence = category.OrderSequence,
                                Jan = jan,
                                Feb = feb,
                                Mar = mar,
                                Apr = apr,
                                May = may,
                                Jun = jun,
                                Jul = jul,
                                Aug = aug,
                                Sep = sep,
                                Oct = oct,
                                Nov = nov,
                                Dec = dec
                            }); ;
                        }
                    }

                    if (errorMessages.Count > 0)
                    {
                        result = new List<AnnualOTPlanningDetailView>();
                        string finalErrorMessages = string.Empty;
                        foreach (string error in errorMessages)
                        {
                            finalErrorMessages = finalErrorMessages + error + separator;
                        }

                        throw new Exception(finalErrorMessages);
                    }
                }
            }

            return result;
        }


        [HttpGet("download-template")]
        public IActionResult DownloadAnnualOTPlanningTemplate(string mode, Guid docApprovalID)
        {
            //ClaimBenefitService.
            using (var documentStream = GenerateAnnualOTPlanningTemplate(mode, docApprovalID))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual OT Planning Template", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public string GetDivisionLabourType(string noreg)
        {
            //default division
            string division = "IDL";
            ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(noreg);
            List<EmployeeOrganizationStoredEntity> subordinates = MdmService.GetEmployeeByOrgCode(org.OrgCode, DateTime.Now.ToString("yyyy-MM-dd")).Where(a => a.NoReg != noreg).ToList();
            foreach (EmployeeOrganizationStoredEntity subordinate in subordinates)
            {
                division = MdmService.GetOrganizationalAssignment(subordinate.NoReg).LabourType;
                if (division == "DL")
                    break;
            }
            return division;
        }

        public MemoryStream GenerateAnnualOTPlanningTemplate(string mode, Guid docApprovalID)
        {
            try
            {
                string noReg = User.Claims.ElementAtOrDefault(0).Value;
                string nooreg = ServiceProxy.UserClaim.NoReg;
                string name = User.Claims.ElementAtOrDefault(2).Value;
                string division = GetDivisionLabourType(nooreg);

                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                string templateDocument;

                ActualOrganizationStructure org = MdmService.GetActualOrganizationStructure(nooreg);
                List<EmployeeOrganizationStoredEntity> subordinates = MdmService.GetEmployeeByOrgCode(org.OrgCode, DateTime.Now.ToString("yyyy-MM-dd")).Where(a => a.NoReg != noReg).ToList();
                bool isDL = false;
                foreach (EmployeeOrganizationStoredEntity subordinate in subordinates)
                {
                    division = MdmService.GetOrganizationalAssignment(subordinate.NoReg).LabourType;
                    if (division == "DL")
                        isDL = true;
                }

                if (isDL)
                    templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-ot-planning-dl.xlsx");
                else
                    templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-ot-planning-idl.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[0];
                        ws.Cells["A2"].Value = noReg + " - " + name;
                        
                        //ADD DATA EXISTING IF DOWNLOAD FOR EDIT
                        if(mode == "edit" && docApprovalID != null)
                        {
                            int rowStart = 4;

                            var data = AnnualOTPlanningService.GetAnnualOTPlanningSummaryExcelFileData(docApprovalID).ToList();
                            var details = AnnualOTPlanningService.GetDetails(docApprovalID);
                            var header = AnnualOTPlanningService.GetAnnualOTPlanningByDocumentApprovalId(docApprovalID).FirstOrDefault();

                            var currentData = details.Where(d => d.CategoryCode.ToLower().Contains("overtimeplanning") && d.LabourType == division).FirstOrDefault();
                            
                            ws.Cells["B" + rowStart.ToString()].Value = currentData.Jan;
                            ws.Cells["C" + rowStart.ToString()].Value = currentData.Feb;
                            ws.Cells["D" + rowStart.ToString()].Value = currentData.Mar;
                            ws.Cells["E" + rowStart.ToString()].Value = currentData.Apr;
                            ws.Cells["F" + rowStart.ToString()].Value = currentData.May;
                            ws.Cells["G" + rowStart.ToString()].Value = currentData.Jun;
                            ws.Cells["H" + rowStart.ToString()].Value = currentData.Jul;
                            ws.Cells["I" + rowStart.ToString()].Value = currentData.Aug;
                            ws.Cells["J" + rowStart.ToString()].Value = currentData.Sep;
                            ws.Cells["K" + rowStart.ToString()].Value = currentData.Nov;
                            ws.Cells["L" + rowStart.ToString()].Value = currentData.Oct;
                            ws.Cells["M" + rowStart.ToString()].Value = currentData.Dec;
                        }

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
        public IActionResult DownloadAnnualOTPlanningSummary(Guid documentApprovalId)
        {
            using (var documentStream = GenerateAnnualOTPlanningSummaryExcelFile(documentApprovalId))
            {
                // Make Sure Document is Loaded
                if (documentStream != null && documentStream.Length > 0)
                {
                    string documentName = string.Format("{0}-{1}.xlsx", "Annual OT Planning Summary", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStream.Position = 0;

                    return File(documentStream.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public MemoryStream GenerateAnnualOTPlanningSummaryExcelFile(Guid documentApprovalId)
        {
            var data = AnnualOTPlanningService.GetAnnualOTPlanningSummaryExcelFileData(documentApprovalId).ToList();
            var details = AnnualOTPlanningService.GetDetails(documentApprovalId);
            var header = AnnualOTPlanningService.GetAnnualOTPlanningByDocumentApprovalId(documentApprovalId).FirstOrDefault();
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\annual-ot-planning-summary.xlsx");

            MemoryStream output = new MemoryStream();
            using (FileStream documentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(documentStream))
                {
                    if (data.Count() > 0)
                    {
                        int rowStart = 3;
                        foreach(AnnualOTPlanningSummaryExcelFileStoredEntity currentPlan in data.Where(x=>x.Version == header.Version).ToList())
                        {
                            ExcelWorksheet sheet = package.Workbook.Worksheets[0];

                            sheet.Cells["A" + rowStart.ToString()].Value = currentPlan.Jan;
                            sheet.Cells["B" + rowStart.ToString()].Value = currentPlan.Feb;
                            sheet.Cells["C" + rowStart.ToString()].Value = currentPlan.Mar;
                            sheet.Cells["D" + rowStart.ToString()].Value = currentPlan.Apr;
                            sheet.Cells["E" + rowStart.ToString()].Value = currentPlan.May;
                            sheet.Cells["F" + rowStart.ToString()].Value = currentPlan.Jun;
                            sheet.Cells["G" + rowStart.ToString()].Value = currentPlan.Jul;
                            sheet.Cells["H" + rowStart.ToString()].Value = currentPlan.Aug;
                            sheet.Cells["I" + rowStart.ToString()].Value = currentPlan.Sep;
                            sheet.Cells["J" + rowStart.ToString()].Value = currentPlan.Nov;
                            sheet.Cells["K" + rowStart.ToString()].Value = currentPlan.Oct;
                            sheet.Cells["L" + rowStart.ToString()].Value = currentPlan.Dec;

                            if (data.Count() > 1)
                            {
                                AnnualOTPlanningSummaryExcelFileStoredEntity prevPlan = data.Where(x=> x.Version == (currentPlan.Version-1)).FirstOrDefault();

                                if (prevPlan != null)
                                {
                                    if (prevPlan.Jan != currentPlan.Jan)
                                    {
                                        sheet.Cells["A" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["A" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Feb != currentPlan.Feb)
                                    {
                                        sheet.Cells["B" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["B" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Mar != currentPlan.Mar)
                                    {
                                        sheet.Cells["C" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["C" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Apr != currentPlan.Apr)
                                    {
                                        sheet.Cells["D" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["D" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.May != currentPlan.May)
                                    {
                                        sheet.Cells["E" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["E" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Jun != currentPlan.Jun)
                                    {
                                        sheet.Cells["F" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["F" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Jul != currentPlan.Jul)
                                    {
                                        sheet.Cells["G" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["G" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Aug != currentPlan.Aug)
                                    {
                                        sheet.Cells["H" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["H" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Sep != currentPlan.Sep)
                                    {
                                        sheet.Cells["I" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["I" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Oct != currentPlan.Oct)
                                    {
                                        sheet.Cells["J" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["J" + rowStart.ToString()].Style.Fill.PatternColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Nov != currentPlan.Nov)
                                    {
                                        sheet.Cells["K" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["K" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
                                    }

                                    if (prevPlan.Dec != currentPlan.Dec)
                                    {
                                        sheet.Cells["L" + rowStart.ToString()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        sheet.Cells["L" + rowStart.ToString()].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 0));
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
    [Route("annual-ot-planning")]
    public class AnnualOTPlanningController : MvcControllerBase
    {
        #region Domain Service

        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        public AnnualOTPlanningService AnnualOTPlanningService => ServiceProxy.GetService<AnnualOTPlanningService>();

        #endregion
        [HttpGet("edit/{documentApprovalId}")]
        public IActionResult Edit(Guid documentApprovalId)
        {
            DocumentApproval documentApproval = ApprovalService.GetDocumentApprovalById(documentApprovalId);
            var model = new DocumentRequestDetailViewModel<AnnualOTPlanningViewModel>();
            if (documentApproval != null)
            {
                model = ApprovalService.GetDocumentRequestDetailViewModel<AnnualOTPlanningViewModel>(documentApprovalId, ServiceProxy.UserClaim.NoReg);
                model.FormKey = "annual-ot-planning";
            }
            else
            {
                return NotFound();
            }

            bool hasNewerVersion = AnnualOTPlanningService.HasNewerVersion(documentApprovalId);

            ViewData.Add("documentApprovalId", documentApproval == null ? null : documentApproval.Id.ToString());
            if (!hasNewerVersion && documentApproval != null)
            {
                ViewData.Add("mode", "edit");
            }

            return View("~/Areas/TimeManagement/Views/Form/AnnualOTPlanning.cshtml", model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }

    #endregion
}
