using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API Controller
    /// <summary>
    /// Complaint request API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ComplaintRequest)]
    [Permission(PermissionKey.CreateComplaintRequest)]
    public class ComplaintRequestApiController : FormApiControllerBase<ComplaintRequestViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Field that hold others service object.
        /// </summary>
        public OthersService OthersService => ServiceProxy.GetService<OthersService>();
        #endregion

        /// <summary>
        /// Ignore default validation when creating complaint object by overriding with empty body.
        /// </summary>
        /// <param name="formKey">This form key.</param>
        protected override void ValidateOnCreate(string formKey) { }

        /// <summary>
        /// Validate before create complaint request object.
        /// </summary>
        /// <param name="requestDetailViewModel">This <see cref="DocumentRequestDetailViewModel"/> object.</param>
        //protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<ComplaintRequestViewModel> requestDetailViewModel)
        //{
        //    base.ValidateOnPostCreate(requestDetailViewModel);

        //    ValidateRequest(requestDetailViewModel);
        //}

        ///// <summary>
        ///// Validate before update complaint request object.
        ///// </summary>
        ///// <param name="requestDetailViewModel">This <see cref="DocumentRequestDetailViewModel"/> object.</param>
        //protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<ComplaintRequestViewModel> requestDetailViewModel)
        //{
        //    base.ValidateOnPostUpdate(requestDetailViewModel);

        //    ValidateRequest(requestDetailViewModel);
        //}

        /// <summary>
        /// Validate complaint request view model.
        /// </summary>
        /// <param name="viewModel">This <see cref="ComplaintRequestViewModel"/> object.</param>
        [HttpPost("validate-request")]
        public ComplaintRequestViewModel ValidateRequest(ComplaintRequestViewModel viewModel)
        {
            return ModelState.IsValid ? viewModel : null;
        }

        /// <summary>
        /// Update solution.
        /// </summary>
        /// <param name="viewModel">This <see cref="ComplaintRequestSolutionViewModel"/> object.</param>
        [HttpPost("solutions")]
        public IActionResult Solutions(ComplaintRequestSolutionViewModel viewModel)
        {
            OthersService.UpdateComplaintRequestSolutions(viewModel);

            return NoContent();
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime startDate, DateTime endDate)
        {
            //var request = new DataSourceRequest
            //{
            //    Filters = new List<IFilterDescriptor>
            //    {
            //        new FilterDescriptor("CreatedOn", FilterOperator.IsGreaterThanOrEqualTo, startDate),
            //        new FilterDescriptor("CreatedOn", FilterOperator.IsLessThanOrEqualTo, endDate)
            //    },
            //    Sorts = new List<SortDescriptor>
            //    {
            //        new SortDescriptor("CreatedOn", ListSortDirection.Ascending),
            //        new SortDescriptor("Name", ListSortDirection.Ascending)
            //    }
            //};

            //var output = ServiceProxy.GetTableValuedDataSourceResult<AyoSekolahReportStoredEntity>(request, new { noreg, username, orgCode }).Data.Cast<AyoSekolahReportStoredEntity>();

            //var fileName = string.Format("COMPLAINT REQUEST-REPORT-{0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            //if (!ServiceProxy.UserClaim.Chief)
            //{
            //    output = output.Where(x => x.NoReg == noreg);
            //}

            //using (var ms = new MemoryStream())
            //{
            //    using (var package = new ExcelPackage())
            //    {
            //        int rowIndex = 2;
            //        var sheet = package.Workbook.Worksheets.Add("Output");
            //        var format = "dd/MM/yyyy";

            //        var cols = new[] { "Noreg", "Name", "Document Number", "Document Status", "Division", "Department", "Section", "Position", "Job", "Class", "Date" };

            //        for (var i = 1; i <= cols.Length; i++)
            //        {
            //            sheet.Cells[1, i].Value = cols[i - 1];
            //            sheet.Cells[1, i].Style.Font.Bold = true;
            //            sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //        }

            //        foreach (var item in output)
            //        {
            //            sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
            //            sheet.Cells[rowIndex, 2].Value = item.Name;
            //            sheet.Cells[rowIndex, 3].Value = item.DocumentNumber;
            //            sheet.Cells[rowIndex, 4].Value = item.DocumentStatusTitle;
            //            sheet.Cells[rowIndex, 5].Value = item.Division;
            //            sheet.Cells[rowIndex, 6].Value = item.Department;
            //            sheet.Cells[rowIndex, 7].Value = item.Section;
            //            sheet.Cells[rowIndex, 8].Value = item.PostName;
            //            sheet.Cells[rowIndex, 9].Value = item.JobName;
            //            sheet.Cells[rowIndex, 10].Value = item.EmployeeSubgroupText;
            //            sheet.Cells[rowIndex, 11].Value = item.CreatedOn.ToString(format);
            //            sheet.Cells[rowIndex, 11].Style.QuotePrefix = true;

            //            for (var i = 1; i <= cols.Length; i++)
            //            {
            //                sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //            }

            //            rowIndex++;
            //        }

            //        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            //        package.SaveAs(ms);
            //    }

            //    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            //}

            return NoContent();
        }

        /// <summary>
        /// Validate request view model.
        /// </summary>
        /// <param name="requestDetailViewModel">This <see cref="DocumentRequestDetailViewModel"/> object.</param>
        //private void ValidateRequest(DocumentRequestDetailViewModel<ComplaintRequestViewModel> requestDetailViewModel)
        //{
        //    Assert.ThrowIf(requestDetailViewModel.Object == null || requestDetailViewModel.Object.Length == 0, "Cannot update request because request is empty");
        //}
    }
    #endregion
}