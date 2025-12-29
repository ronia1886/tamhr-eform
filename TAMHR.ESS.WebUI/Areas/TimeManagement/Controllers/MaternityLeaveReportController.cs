using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Maternity Leave Report API Manager
    /// </summary>
    [Route("api/maternity-leave-report")]
    public class MaternityLeaveReportApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Master data management service object
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        /// <summary>
        /// Get ayo sekolah report document status summary
        /// </summary>
        /// <remarks>
        /// Get ayo sekolah report document status summary
        /// </remarks>
        [HttpPost("document-status-summary")]
        public IEnumerable<dynamic> GetDocumentStatusSummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("CreatedOn BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return ServiceProxy.GetTableValuedSummary<MaternityLeaveStoredEntity>("DocumentStatusCode", "DocumentStatus", new { noreg, username, orgCode }, filter);
        }

        /// <summary>
        /// Get ayo sekolah report class summary
        /// </summary>
        /// <remarks>
        /// Get ayo sekolah report class summary
        /// </remarks>
        [HttpPost("class-summary")]
        public IEnumerable<dynamic> GetClassSummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("CreatedOn BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return ServiceProxy.GetTableValuedSummary<MaternityLeaveStoredEntity>("ClassRange", "ClassRange", new { noreg, username, orgCode }, filter);
        }

        [HttpPost]
        [Route("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;

            return ServiceProxy.GetTableValuedDataSourceResult<MaternityLeaveStoredEntity>(request, new { noreg, username, orgCode });
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime startDate, DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var username = ServiceProxy.UserClaim.Username;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure.OrgCode;
            var request = new DataSourceRequest
            {
                Filters = new List<IFilterDescriptor>
                {
                    new FilterDescriptor("CreatedOn", FilterOperator.IsGreaterThanOrEqualTo, startDate),
                    new FilterDescriptor("CreatedOn", FilterOperator.IsLessThanOrEqualTo, endDate)
                },
                Sorts = new List<SortDescriptor>
                {
                    new SortDescriptor("CreatedOn", ListSortDirection.Ascending),
                    new SortDescriptor("Name", ListSortDirection.Ascending)
                }
            };

            //var output = ServiceProxy.GetTableValuedDataSourceResult<MaternityLeaveReportStoredEntity>(request, new { noreg, username, orgCode }).Data.Cast<MaternityLeaveReportStoredEntity>();
            var dataSourceResult = MdmService.GetMaternityLeaveReport(request, noreg, username, orgCode);
            var output = dataSourceResult.Data.OfType<MaternityLeaveReportStoredEntity>();

            var fileName = string.Format("MATERNITY-LEAVE-REPORT-{0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "Noreg", "Name", "Document Number", "Document Status", "Division", "Department", "Section", "Position", "Job", "Class", "Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var item in output)
                    {
                        sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
                        sheet.Cells[rowIndex, 2].Value = item.Name;
                        sheet.Cells[rowIndex, 3].Value = item.DocumentNumber;
                        sheet.Cells[rowIndex, 4].Value = item.DocumentStatusTitle;
                        sheet.Cells[rowIndex, 5].Value = item.Division;
                        sheet.Cells[rowIndex, 6].Value = item.Department;
                        sheet.Cells[rowIndex, 7].Value = item.Section;
                        sheet.Cells[rowIndex, 8].Value = item.PostName;
                        sheet.Cells[rowIndex, 9].Value = item.JobName;
                        sheet.Cells[rowIndex, 10].Value = item.EmployeeSubgroupText;
                        sheet.Cells[rowIndex, 11].Value = item.CreatedOn.ToString(format);
                        sheet.Cells[rowIndex, 11].Style.QuotePrefix = true;

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
}