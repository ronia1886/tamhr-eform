using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// BDJK Report API Manager
    /// </summary>
    [Route("api/bdjk-report")]
    public class BdjkReportApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Time management service
        /// </summary>
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        /// <summary>
        /// BDJK service
        /// </summary>
        protected BdjkService BdjkService => ServiceProxy.GetService<BdjkService>();

        /// <summary>
        /// Master data management service
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        /// <summary>
        /// Get bdjk report document status summary
        /// </summary>
        /// <remarks>
        /// Get bdjk report document status summary
        /// </remarks>
        [HttpPost("document-status-summary")]
        public IEnumerable<dynamic> GetDocumentStatusSummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var orgLevel = organizationStructure?.OrgLevel;
            var filter = string.Format("WorkingDate BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (!ServiceProxy.UserClaim.Chief)
            {
                filter += string.Format(" AND NoReg = '{0}'", noreg);
            }

            return ServiceProxy.GetTableValuedSummary<BdjkRequestDetailStoredEntity>("DocumentStatusCode", "DocumentStatus", new { noreg, username, orgCode, orgLevel }, filter);
        }

        /// <summary>
        /// Get bdjk report activity summary
        /// </summary>
        /// <remarks>
        /// Get bdjk report activity summary
        /// </remarks>
        [HttpPost("activity-summary")]
        public IEnumerable<dynamic> GetActivitySummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var orgLevel = organizationStructure?.OrgLevel;

            var filter = string.Format("WorkingDate BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (!ServiceProxy.UserClaim.Chief)
            {
                filter += string.Format(" AND NoReg = '{0}'", noreg);
            }

            return ServiceProxy.GetTableValuedSummary<BdjkRequestDetailStoredEntity>("ActivityCode", "CategorySPKL", new { noreg, username, orgCode, orgLevel }, filter);
        }

        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var orgLevel = organizationStructure?.OrgLevel;

            if (!ServiceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            return ServiceProxy.GetTableValuedDataSourceResult<BdjkRequestDetailStoredEntity>(request, new { noreg, username, orgCode, orgLevel });
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime startDate, DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var username = ServiceProxy.UserClaim.Username;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure.OrgCode;
            var orgLevel = organizationStructure.OrgLevel;
            var request = new DataSourceRequest
            {
                Filters = new List<IFilterDescriptor>
                {
                    new FilterDescriptor("WorkingDate", FilterOperator.IsGreaterThanOrEqualTo, startDate),
                    new FilterDescriptor("WorkingDate", FilterOperator.IsLessThanOrEqualTo, endDate)
                },
                Sorts = new List<SortDescriptor>
                {
                    new SortDescriptor("WorkingDate", ListSortDirection.Ascending),
                    new SortDescriptor("Name", ListSortDirection.Ascending)
                }
            };

            if (!ServiceProxy.UserClaim.Chief)
            {
                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            //var output = ServiceProxy.GetTableValuedDataSourceResult<BdjkRequestDetailReportStoredEntity>(request, new { noreg, username, orgCode, orgLevel }).Data.OfType<BdjkRequestDetailReportStoredEntity>();
            var dataSourceResult = BdjkService.GetBdjkReportDetails(request, noreg, username, orgCode, orgLevel);
            var output = dataSourceResult.Data.OfType<BdjkRequestDetailReportStoredEntity>();

            var fileName = string.Format("BDJK-REPORT-{0:ddMMyyyy}.xlsx", DateTime.Now);

            if (!ServiceProxy.UserClaim.Chief)
            {
                output = output.Where(x => x.NoReg == noreg);
            }

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No", "Noreg", "Name", "Division", "Department", "Section", "Document Number", "Status", "Date", "Proxy In", "Proxy Out", "BDJK Code", "Duration", "Activity", "Reason" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var item in output)
                    {
                        sheet.Cells[rowIndex, 1].Value = rowIndex - 1;
                        sheet.Cells[rowIndex, 2].Value = int.Parse(item.NoReg);
                        sheet.Cells[rowIndex, 3].Value = item.Name;
                        sheet.Cells[rowIndex, 4].Value = item.Division;
                        sheet.Cells[rowIndex, 5].Value = item.Department;
                        sheet.Cells[rowIndex, 6].Value = item.Section;
                        sheet.Cells[rowIndex, 7].Value = item.ParentDocumentNumber != null ? item.ParentDocumentNumber : "Master Data";
                        sheet.Cells[rowIndex, 8].Value = item.DocumentStatusName;
                        sheet.Cells[rowIndex, 9].Value = item.WorkingDate.ToString(format);
                        sheet.Cells[rowIndex, 9].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 10].Value = item.WorkingTimeIn.HasValue ? item.WorkingTimeIn.Value.ToString("HH:mm") : string.Empty;
                        sheet.Cells[rowIndex, 10].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 11].Value = item.WorkingTimeOut.HasValue ? item.WorkingTimeOut.Value.ToString("HH:mm") : string.Empty;
                        sheet.Cells[rowIndex, 11].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 12].Value = item.BdjkCode;
                        sheet.Cells[rowIndex, 13].Value = item.Duration;
                        sheet.Cells[rowIndex, 14].Value = item.ActivityName;
                        sheet.Cells[rowIndex, 15].Value = item.BdjkReason;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class BdjkReportController : MvcControllerBase
    {
        public IActionResult index()
        {
            return View();
        }
    }
    #endregion
}