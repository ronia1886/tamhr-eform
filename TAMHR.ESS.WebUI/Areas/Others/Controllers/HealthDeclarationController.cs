using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    [Route("api/health-declaration-report")]
    [Permission(PermissionKey.ViewHealthDeclarationReport)]
    public class HealthDeclarationReportApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// MDM service object.
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        [HttpGet("download")]
        public async Task<IActionResult> Download(DateTime submissionDate)
        {
            var now = DateTime.Now;
            var request = new DataSourceRequest
            {
                Sorts = new List<SortDescriptor>
                {
                    new SortDescriptor("Name", ListSortDirection.Ascending)
                }
            };

            var canViewAll = AclHelper.HasPermission("Core.ViewAllHealthDeclarationReport");
            var formService = ServiceProxy.GetService<FormService>();
            var summary = ServiceProxy.GetService<OthersService>().GetHealthDeclarationSummaries(submissionDate);
            var all = (await GetReport(submissionDate, request)).Data as IEnumerable<HealthDeclarationReportStoredEntity>;
            var submitted = all.Where(x => x.HasSubmitForm);
            var needMonitoring = submitted.Where(x => x.IsSick ?? false);
            var notSubmitted = all.Where(x => !x.HasSubmitForm);
            var needToFollowUp = all.Where(x => x.HasRemarks);
            var fileName = string.Format("HEALTH_DECLARATION-REPORT-{0:ddMMyyyy}.xlsx", submissionDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    PrintNotSubmitted(package, notSubmitted);
                    PrintSubmitted("Raw Data", package, submitted);

                    if (canViewAll)
                    {
                        PrintFollowUp(package, needToFollowUp);
                        PrintSubmitted("Need Monitoring", package, needMonitoring);
                        PrintSummary(package, summary);
                    }

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        /// <summary>
        /// Get health declarations history.
        /// </summary>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-history")]
        public async Task<DataSourceResult> GetHistory([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            var service = ServiceProxy.GetService<OthersService>();

            return await service.GetHealthDeclarations(noreg)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get health declaration report by submission date.
        /// </summary>
        /// <param name="submissionDate">This submission date.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-report")]
        public async Task<DataSourceResult> GetReport([FromForm] DateTime submissionDate, [DataSourceRequest] DataSourceRequest request)
        {
            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Get and set position code from current user session.
            var postCode = ServiceProxy.UserClaim.PostCode;

            var canViewAll = AclHelper.HasPermission("Core.ViewAllHealthDeclarationReport");

            var canViewDivisionReport = AclHelper.HasPermission("Core.ViewHealthDeclarationDivisionReport");

            // Get organization structure object from given current user session noreg and position code.
            var organizationStructure = MdmService.GetOrganizationLevel(noreg, postCode, canViewDivisionReport ? "Division" : null);

            // Create new anonymous parameters.
            var parameters = new
            {
                // Get and set actor.
                actor = noreg,
                // Get and set submission date.
                submissionDate,
                // Get and set organization code.
                orgCode = canViewAll ? "*" : organizationStructure.OrgCode,
                // Get and set organization level.
                orgLevel = canViewAll ? 0 : organizationStructure.OrgLevel
            };

            // Create dynamic result from table valued query with given request and parameters.
            return await Task.FromResult(ServiceProxy.GetTableValuedDataSourceResult<HealthDeclarationReportStoredEntity>(request, parameters));
        }

        /// <summary>
        /// Get report summary (sick and non-sick) by submission date.
        /// </summary>
        /// <param name="submissionDate">This submission date.</param>
        /// <returns>This list of dynamic object.</returns>
        [HttpPost("summary")]
        public async Task<IEnumerable<dynamic>> GetSummary([FromForm]DateTime submissionDate)
        {
            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Get and set position code from current user session.
            var postCode = ServiceProxy.UserClaim.PostCode;

            var canViewAll = AclHelper.HasPermission("Core.ViewAllHealthDeclarationReport");

            var canViewDivisionReport = AclHelper.HasPermission("Core.ViewHealthDeclarationDivisionReport");

            // Get organization structure object from given current user session noreg and position code.
            var organizationStructure = MdmService.GetOrganizationLevel(noreg, postCode, canViewDivisionReport ? "Division" : null);

            // Create new anonymous parameters.
            var parameters = new
            {
                // Get and set actor.
                actor = noreg,
                // Get and set submission date.
                submissionDate,
                // Get and set organization code.
                orgCode = canViewAll ? "*" : organizationStructure.OrgCode,
                // Get and set organization level.
                orgLevel = canViewAll ? 0 : organizationStructure.OrgLevel
            };

            // Create dynamic summary query from given parameters and filter.
            // The field WorkTypeCode will be compare and map with code in general category with "WorkingType" as category.
            return await Task.FromResult(ServiceProxy.GetTableValuedSummary<HealthDeclarationReportStoredEntity>("HealthTypeCode", "HealthType", parameters));
        }

        /// <summary>
        /// Get working type report summary by submission date.
        /// </summary>
        /// <param name="submissionDate">This submission date.</param>
        /// <returns>This list of dynamic object.</returns>
        [HttpPost("work-type-summary")]
        public async Task<IEnumerable<dynamic>> GetWorkTypeSummary([FromForm]DateTime submissionDate)
        {
            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Get and set position code from current user session.
            var postCode = ServiceProxy.UserClaim.PostCode;

            var canViewAll = AclHelper.HasPermission("Core.ViewAllHealthDeclarationReport");

            var canViewDivisionReport = AclHelper.HasPermission("Core.ViewHealthDeclarationDivisionReport");

            // Get organization structure object from given current user session noreg and position code.
            var organizationStructure = MdmService.GetOrganizationLevel(noreg, postCode, canViewDivisionReport ? "Division" : null);

            // Setup additional string filter by submission date (always set in UTC format when the value type is a date).
            var filter = string.Format("SubmissionDate = '{0}'", submissionDate.ToString("yyyy-MM-dd"));

            // Create new anonymous parameters.
            var parameters = new
            {
                // Get and set actor.
                actor = noreg,
                // Get and set submission date.
                submissionDate,
                // Get and set organization code.
                orgCode = canViewAll ? "*" : organizationStructure.OrgCode,
                // Get and set organization level.
                orgLevel = canViewAll ? 0 : organizationStructure.OrgLevel
            };

            // Create dynamic summary query from given parameters and filter.
            // The field WorkTypeCode will be compare and map with code in general category with "WorkingType" as category.
            return await Task.FromResult(ServiceProxy.GetTableValuedSummary<HealthDeclarationReportStoredEntity>("WorkTypeCode", "WorkingType", parameters, filter));
        }

        [HttpPost("change-status")]
        public IActionResult ChangeStatus([FromBody] GenericRequest<Guid> genericRequest)
        {
            var service = ServiceProxy.GetService<OthersService>();

            var output = service.MarkedHealthDeclaration(genericRequest.Value, true);

            return Ok(output);
        }

        #region Private Methods
        private void PrintSummary(ExcelPackage package, IEnumerable<HealthDeclarationSummaryStoredEntity> items)
        {
            var rowIndex = 2;
            var sheet = package.Workbook.Worksheets.Add("WFO WFH PERCENTAGE");

            var columns = new[] {
                "Division",
                "Total MP",
                "Employee WFO",
                "Employee WFH",
                "Total Not Submitted",
                "Total Submitted"
            };

            for (var i = 1; i <= columns.Length; i++)
            {
                sheet.Cells[1, i].Value = columns[i - 1];
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }


            foreach (var item in items)
            {
                var col = 1;

                sheet.Cells[rowIndex, col++].Value = item.Division;

                sheet.Cells[rowIndex, col++].Value = item.Total;

                sheet.Cells[rowIndex, col++].Value = item.TotalWorkFromOffice;

                sheet.Cells[rowIndex, col++].Value = item.TotalWorkFromHome;

                sheet.Cells[rowIndex, col++].Value = item.TotalNotSubmitted;

                if (item.TotalNotSubmitted > 0)
                {
                    sheet.Cells[rowIndex, col - 1].Style.Font.Color.SetColor(Color.Red);
                }

                sheet.Cells[rowIndex, col++].Formula = string.Format("=(C{0}+D{0})/B{0}", rowIndex);
                sheet.Cells[rowIndex, col - 1].Style.Numberformat.Format = "0%";

                for (var i = 1; i <= columns.Length; i++)
                {
                    sheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private void PrintFollowUp(ExcelPackage package, IEnumerable<HealthDeclarationReportStoredEntity> items)
        {
            var rowIndex = 2;
            var sheet = package.Workbook.Worksheets.Add("Need to Follow Up");

            var columns = new[]
            {
                "No Reg.",
                "Name",
                "Position",
                "Job",
                "Email",
                "Phone Number",
                "Emergency Family Status",
                "Emergency Name",
                "Emergency Phone Number",
                "Hierarchy Name",
                "Hierarchy Email",
                "Hierarchy Phone Number",
                "Division",
                "Departement",
                "Section",
                "Working Type",
                "Notes"
            };

            for (var i = 1; i <= columns.Length; i++)
            {
                sheet.Cells[1, i].Value = columns[i - 1];
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            foreach (var item in items)
            {
                var col = 1;

                sheet.Cells[rowIndex, col++].Value = int.Parse(item.NoReg);
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[rowIndex, col++].Value = item.Name;
                sheet.Cells[rowIndex, col++].Value = item.PostName;
                sheet.Cells[rowIndex, col++].Value = item.JobName;
                sheet.Cells[rowIndex, col++].Value = item.Email;
                sheet.Cells[rowIndex, col++].Value = item.PhoneNumber;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyFamilyStatus;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyName;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyPhoneNumber;
                sheet.Cells[rowIndex, col++].Value = item.HierarchyName;
                sheet.Cells[rowIndex, col++].Value = item.HierarchyEmail;
                sheet.Cells[rowIndex, col++].Value = item.HierarchyPhoneNumber;
                sheet.Cells[rowIndex, col++].Value = item.Division;
                sheet.Cells[rowIndex, col++].Value = item.Department;
                sheet.Cells[rowIndex, col++].Value = item.Section;
                sheet.Cells[rowIndex, col++].Value = item.WorkType;
                sheet.Cells[rowIndex, col++].Value = item.Notes;

                for (var i = 1; i <= columns.Length; i++)
                {
                    sheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private void PrintNotSubmitted(ExcelPackage package, IEnumerable<HealthDeclarationReportStoredEntity> items)
        {
            var rowIndex = 2;

            var sheet = package.Workbook.Worksheets.Add("List not Submit");

            var columns = new[]
            {
                "No Reg.",
                "Name",
                "Position",
                "Job",
                "Email",
                "Division",
                "Departement",
                "Section"
            };

            for (var i = 1; i <= columns.Length; i++)
            {
                sheet.Cells[1, i].Value = columns[i - 1];
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            foreach (var item in items)
            {
                var col = 1;

                sheet.Cells[rowIndex, col++].Value = int.Parse(item.NoReg);
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[rowIndex, col++].Value = item.Name;
                sheet.Cells[rowIndex, col++].Value = item.PostName;
                sheet.Cells[rowIndex, col++].Value = item.JobName;
                sheet.Cells[rowIndex, col++].Value = item.Email;
                sheet.Cells[rowIndex, col++].Value = item.Division;
                sheet.Cells[rowIndex, col++].Value = item.Department;
                sheet.Cells[rowIndex, col++].Value = item.Section;

                for (var i = 1; i <= columns.Length; i++)
                {
                    sheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private void PrintSubmitted(string sheetName, ExcelPackage package, IEnumerable<HealthDeclarationReportStoredEntity> items)
        {
            var formService = ServiceProxy.GetService<FormService>();
            var rowIndex = 2;
            var sheet = package.Workbook.Worksheets.Add(sheetName);
            var viewModels = items.Select(x => JsonConvert.DeserializeObject<HealthDeclarationViewModel>(x.ObjectValue)).ToArray();

            var formQuestionIds = viewModels.SelectMany(x => x.FormAnswers)
                .Select(x => x.FormQuestionId)
                .Distinct();

            //var questions = formService.GetFormQuestions(formQuestionIds);

            IEnumerable<FormQuestion> questions = new List<FormQuestion>();

            if (formQuestionIds.Any())
            {
                questions = formService.GetFormQuestions(formQuestionIds);
            }

            var fields = new List<string> {
                "Submission Date",
                "Timestamp",
                "No Reg.",
                "Name",
                "Position",
                "Job",
                "Division",
                "Department",
                "Section",
                "Phone Number",
                "Email",
                "Has been Submitted Form",
                "Category",
                "Notes",
                "Have Fever",
                "Body Temperature",
                "Working Type",
                "Emergency Family Status",
                "Emergency Name",
                "Emergency Phone Number"
            };

            questions.ForEach(x => fields.Add(x.Title));

            var columns = fields.ToArray();

            for (var i = 1; i <= columns.Length; i++)
            {
                sheet.Cells[1, i].Value = columns[i - 1];
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            var idx = 0;
            foreach (var item in items)
            {
                var col = 1;
                var viewModel = viewModels[idx++];

                sheet.Cells[rowIndex, col++].Value = item.SubmissionDate.Value.ToString("dd.MM.yyyy");
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = item.CreatedOn.Value.ToString("HH:mm");
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = int.Parse(item.NoReg);
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = item.Name;

                sheet.Cells[rowIndex, col++].Value = item.PostName;

                sheet.Cells[rowIndex, col++].Value = item.JobName;

                sheet.Cells[rowIndex, col++].Value = item.Division;

                sheet.Cells[rowIndex, col++].Value = item.Department;

                sheet.Cells[rowIndex, col++].Value = item.Section;

                sheet.Cells[rowIndex, col++].Value = item.PhoneNumber;

                sheet.Cells[rowIndex, col++].Value = item.Email;

                sheet.Cells[rowIndex, col++].Value = (item.HasSubmitForm) ? "Ya" : "Tidak";
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var category = item.HasRemarks ? "Follow Up" : ((item.IsSick ?? false) ? "Need Monitoring" : "Healthy");

                sheet.Cells[rowIndex, col++].Value = category;
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = item.Notes;

                sheet.Cells[rowIndex, col++].Value = (viewModel.HaveFever ?? false) ? "Ya" : "Tidak";
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = viewModel.BodyTemperature == "others" ? viewModel.BodyTemperatureOtherValue : viewModel.BodyTemperature;
                sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[rowIndex, col++].Value = item.WorkType;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyFamilyStatus;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyName;
                sheet.Cells[rowIndex, col++].Value = item.EmergencyPhoneNumber;

                questions.ForEach(x =>
                {
                    var answer = viewModel.FormAnswers.FirstOrDefault(y => y.FormQuestionId == x.Id);

                    sheet.Cells[rowIndex, col++].Value = answer == null ? string.Empty : (answer?.Value == "true" ? "Ya" : "Tidak");
                    sheet.Cells[rowIndex, col - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                });

                for (var i = 1; i <= columns.Length; i++)
                {
                    sheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    if ((item.IsSick ?? false) || item.HasRemarks)
                    {
                        sheet.Cells[rowIndex, i].Style.Font.Color.SetColor(Color.Red);
                    }
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }
        #endregion
    }

    #region API Controller
    /// <summary>
    /// Health declaration API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.HealthDeclaration)]
    [Permission(PermissionKey.CreateHealthDeclaration)]
    public class HealthDeclarationApiController : FormApiControllerBase<HealthDeclarationViewModel>
    {
        /// <summary>
        /// Validate when accessing create page.
        /// </summary>
        /// <param name="formKey">This form key.</param>
        protected override void ValidateOnCreate(string formKey)
        {
            // Call base ValidateOnCreate method.
            base.ValidateOnCreate(formKey);

            // Get and set FormService from DI container.
            var formService = ServiceProxy.GetService<FormService>();

            // Validate form validity.
            formService.ValidateCreateForm(formKey);

            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Get and set OthersService from DI container.
            var othersService = ServiceProxy.GetService<OthersService>();

            // Validate only one submission per day.
            othersService.PreValidateHealthDeclaration(noreg);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Health declaration MVC controller.
    /// </summary>
    [Area(ApplicationModule.Others)]
    [Permission(PermissionKey.ViewHealthDeclarationReport)]
    public class HealthDeclarationController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service object.
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        #endregion

        /// <summary>
        /// Health declaration report page.
        /// </summary>
        public IActionResult Report()
        {
            // Return the respected view.
            return View();
        }

        [HttpPost]
        public IActionResult Load(Guid id, string noreg)
        {
            var model = ApprovalService.GetDocumentRequestDetailViewModel<HealthDeclarationViewModel>(id, noreg);
            model.FormKey = "health-declaration";

            return PartialView("_HealthDeclarationLoader", model);
        }
    }
    #endregion
}