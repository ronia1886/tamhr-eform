using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using Rotativa.AspNetCore;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Ayo Sekolah Report API Manager
    /// </summary>
    [Route("api/claim-benefit/ayo-sekolah-report")]
    [Permission(PermissionKey.ViewAyoSekolahReport)]
    public class AyoSekolahReportApiController : ApiControllerBase
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

            return ServiceProxy.GetTableValuedSummary<AyoSekolahStoredEntity>("DocumentStatusCode", "DocumentStatus", new { noreg, username, orgCode }, filter);
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

            return ServiceProxy.GetTableValuedSummary<AyoSekolahStoredEntity>("ClassRange", "ClassRange", new { noreg, username, orgCode }, filter);
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

            return ServiceProxy.GetTableValuedDataSourceResult<AyoSekolahStoredEntity>(request, new { noreg, username, orgCode });
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

            //var output = ServiceProxy.GetTableValuedDataSourceResult<AyoSekolahReportStoredEntity>(request, new { noreg, username, orgCode }).Data.Cast<AyoSekolahReportStoredEntity>();
            var output = MdmService.GetAyoSekolahReport(noreg, username, orgCode);

            var fileName = string.Format("AYO-SEKOLAH-REPORT-{0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

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

                    var cols = new[] { "No Reg.", "Name", "Document Number", "Document Status", "Division", "Department", "Section", "Position", "Job", "Class", "Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var item in output)
                    {
                        sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
                        sheet.Cells[rowIndex, 2].Value = "'" + item.Name;
                        sheet.Cells[rowIndex, 3].Value = "'" + item.DocumentNumber;
                        sheet.Cells[rowIndex, 4].Value = "'" + item.DocumentStatusTitle;
                        sheet.Cells[rowIndex, 5].Value = "'" + item.Division;
                        sheet.Cells[rowIndex, 6].Value = "'" + item.Department;
                        sheet.Cells[rowIndex, 7].Value = "'" + item.Section;
                        sheet.Cells[rowIndex, 8].Value = "'" + item.PostName;
                        sheet.Cells[rowIndex, 9].Value = item.JobName.ToString();
                        sheet.Cells[rowIndex, 10].Value = item.EmployeeSubgroupText.ToString();
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

    #region MVC Controller
    [Area(ApplicationModule.ClaimBenefit)]
    [Permission(PermissionKey.ViewAyoSekolahReport)]
    public class AyoSekolahReportController : MvcControllerBase
    {
        #region Domain Services
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        public IActionResult Index()
        {
            return View();
        }

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

            var output = ServiceProxy.GetTableValuedDataSourceResult<AyoSekolahReportStoredEntity>(request, new { noreg, username, orgCode }).Data.Cast<AyoSekolahReportStoredEntity>();

            var title = "Ayo Sekolah Report - " + DateTime.Now.ToString("dd-MM-yyyy");
            var customSwitches = string.Format(
                                     "--title \"{0}\" " +
                                     "--header-html \"{1}\" " +
                                     "--header-spacing \"0\" " +
                                     "--header-font-size \"9\" ",
                                     title, GetHeaderPdfUrl("/core/default/commonheader?hide=1")
                                 );

            return new ViewAsPdf("PdfReportForm", new Infrastructure.ViewModels.PdfViewModel()
            {
                Object = output,
                ViewBag = ViewBag
            })
            {
                ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                FileName = title + ".pdf",
                CustomSwitches = customSwitches,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(30, 15, 10, 15)
            };
        }
    }
    #endregion
}