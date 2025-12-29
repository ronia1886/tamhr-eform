using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using Rotativa.AspNetCore;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    [Route("api/spkl-report")]
    public class SpklReportApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Time management service object.
        /// </summary>
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        /// <summary>
        /// SPKL service object.
        /// </summary>
        protected SpklService SpklService => ServiceProxy.GetService<SpklService>();

        /// <summary>
        /// MDM service object.
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        private DateTime ParseDate(string input)
        {
            DateTime output;

            if (DateTime.TryParse(input, out output))
            {
                return output;
            }

            return new DateTime(1900, 1, 1);
        }

        [HttpPost]
        [Route("update")]
        public IActionResult Update([FromBody] SpklOvertimeUpdateViewModel viewModel)
        {
            SpklService.UpdateSpkl(viewModel);

            return NoContent();
        }

        [HttpPost]
        [Route("update-detail")]
        public IActionResult UpdateDetail(SpklOvertimeUpdateViewModel viewModel)
        {
            SpklService.UpdateSpklDetail(viewModel);

            return NoContent();
        }

        [HttpPost]
        [Route("update-multiple")]
        public IActionResult UpdateMultiple([FromBody] SpklOvertimeUpdateViewModel[] viewModel)
        {
            SpklService.UpdateSpkl(viewModel);

            return NoContent();
        }

        [HttpGet]
        [Route("get-documents")]
        public IEnumerable<SpklRequestDetailDocumentNumberStoredEntity> GetDocuments(DateTime keyDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var username = ServiceProxy.UserClaim.Username;
            var month = keyDate.Month;
            var year = keyDate.Year;

            return SpklService.GetSpklRequestDetailDocumentNumbers(noreg, username, month, year);
        }

        [HttpGet]
        [Route("get-dates")]
        public IEnumerable<string> GetCompletedDates(Guid parentId)
        {
            return SpklService.GetCompletedSpklRequestDates(parentId);
        }

        [HttpGet]
        [Route("get-plan-dates")]
        public IEnumerable<string> GetDates(Guid parentId)
        {
            return SpklService.GetSpklRequestDates(parentId);
        }

        /// <summary>
        /// Get spkl report document status summary
        /// </summary>
        /// <remarks>
        /// Get spkl report document status summary
        /// </remarks>
        [HttpPost("document-status-summary")]
        public IEnumerable<dynamic> GetDocumentStatusSummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("OvertimeDate BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (!ServiceProxy.UserClaim.Chief)
            {
                filter += string.Format(" AND NoReg = '{0}'", noreg);
            }

            return ServiceProxy.GetTableValuedSummary<SpklRequestDetailStoredEntity>("DocumentStatusCode", "DocumentStatus", new { noreg, username, orgCode }, filter);
        }

        /// <summary>
        /// Get spkl report activity summary
        /// </summary>
        /// <remarks>
        /// Get spkl report activity summary
        /// </remarks>
        [HttpPost("activity-summary")]
        public IEnumerable<dynamic> GetActivitySummary([FromForm]DateTime startDate, [FromForm]DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("OvertimeDate BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (!ServiceProxy.UserClaim.Chief)
            {
                filter += string.Format(" AND NoReg = '{0}'", noreg);
            }

            return ServiceProxy.GetTableValuedSummary<SpklRequestDetailStoredEntity>("OvertimeCategoryCode", "CategorySPKL", new { noreg, username, orgCode }, filter);
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

            if (!ServiceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            var result = ServiceProxy.GetTableValuedDataSourceResult<SpklRequestDetailStoredEntity>(request, new { noreg, username, orgCode });

            // Debug: Log raw data sebelum serialization
            foreach (var item in result.Data.Cast<SpklRequestDetailStoredEntity>())
            {
                Console.WriteLine($"DB OvertimeIn: {item.OvertimeIn}");
                Console.WriteLine($"DB OvertimeIn Kind: {item.OvertimeIn.Kind}");
                Console.WriteLine($"DB OvertimeIn ToString: {item.OvertimeIn:yyyy-MM-ddTHH:mm:ss}");
            }

            return ServiceProxy.GetTableValuedDataSourceResult<SpklRequestDetailStoredEntity>(request, new { noreg, username, orgCode });
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
                    new FilterDescriptor("OvertimeDate", FilterOperator.IsGreaterThanOrEqualTo, startDate),
                    new FilterDescriptor("OvertimeDate", FilterOperator.IsLessThanOrEqualTo, endDate)
                },
                Sorts = new List<SortDescriptor>
                {
                    new SortDescriptor("OvertimeDate", ListSortDirection.Ascending),
                    new SortDescriptor("Name", ListSortDirection.Ascending)
                }
            };

            if (!ServiceProxy.UserClaim.Chief)
            {
                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            //var output = ServiceProxy.GetTableValuedDataSourceResult<SpklRequestDetailReportStoredEntity>(request, new { noreg, username, orgCode }).Data.Cast<SpklRequestDetailReportStoredEntity>();
            var dataSourceResult = SpklService.GetSpklReportDetails(request, noreg, username, orgCode);
            var output = dataSourceResult.Data.OfType<SpklRequestDetailReportStoredEntity>();

            var fileName = string.Format("SPKL-REPORT-{0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

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

                    var cols = new[] { "Noreg", "Name", "Document Number", "Status", "Division", "Department", "Section", "Overtime Date", "Overtime In", "Overtime Out", "Overtime Break", "Overtime Duration", "Overtime Category", "Overtime Reason" };

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
                        sheet.Cells[rowIndex, 3].Value = item.ParentDocumentNumber != null ? item.ParentDocumentNumber : "Master Data";
                        sheet.Cells[rowIndex, 4].Value = item.DocumentStatusTitle;
                        sheet.Cells[rowIndex, 5].Value = item.Division;
                        sheet.Cells[rowIndex, 6].Value = item.Department;
                        sheet.Cells[rowIndex, 7].Value = item.Section;
                        sheet.Cells[rowIndex, 8].Value = item.OvertimeDate.ToString(format);
                        sheet.Cells[rowIndex, 8].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 9].Value = item.OvertimeInAdjust.HasValue ? item.OvertimeInAdjust.Value.ToString("HH:mm") : item.OvertimeIn.ToString("HH:mm");
                        sheet.Cells[rowIndex, 9].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 10].Value = item.OvertimeOutAdjust.HasValue ? item.OvertimeOutAdjust.Value.ToString("HH:mm") : item.OvertimeOut.ToString("HH:mm");
                        sheet.Cells[rowIndex, 10].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 11].Value = item.OvertimeBreakAdjust;
                        sheet.Cells[rowIndex, 12].Value = item.DurationAdjust.HasValue ? item.DurationAdjust : item.Duration;
                        sheet.Cells[rowIndex, 13].Value = item.OvertimeCategory;
                        sheet.Cells[rowIndex, 14].Value = item.OvertimeReason;

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

        [HttpGet("download-template")]
        public IActionResult Download()
        {
            var keyDate = DateTime.Now.Date;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;

            var data = SpklService.GetSpklRequestDetailsByUser(noreg, username, orgCode).Where(x => x.EnableDocumentAction && x.DocumentStatusCode=="inprogress").OrderBy(x => x.Name).ThenBy(x => x.OvertimeDate);
            var fileName = string.Format("SPKL-ADJUST-TEMPLATE-{0:ddMMyyyy}.xlsx", keyDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "Id", "Noreg", "Name", "Overtime Date", "Overtime In Adjust", "Overtime Out Adjust", "Overtime Break Adjust", "Overtime Category", "Overtime Reason" };

                    sheet.Column(1).Hidden = true;

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var item in data)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.Id;
                        sheet.Cells[rowIndex, 2].Value = int.Parse(item.NoReg);
                        sheet.Cells[rowIndex, 3].Value = item.Name;
                        sheet.Cells[rowIndex, 4].Value = item.OvertimeDate.ToString(format);
                        sheet.Cells[rowIndex, 4].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 5].Value = item.OvertimeInAdjust.HasValue ? item.OvertimeInAdjust.Value.ToString("HH:mm") : "";
                        sheet.Cells[rowIndex, 5].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 6].Value = item.OvertimeOutAdjust.HasValue ? item.OvertimeOutAdjust.Value.ToString("HH:mm") : "";
                        sheet.Cells[rowIndex, 6].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 7].Value = item.OvertimeBreakAdjust ?? 0;
                        sheet.Cells[rowIndex, 8].Value = item.OvertimeCategory;
                        sheet.Cells[rowIndex, 9].Value = item.OvertimeReason;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost("merge")]
        public async Task<IActionResult> Upload()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var username = ServiceProxy.UserClaim.Username;
            var file = Request.Form.Files[0];
            var now = DateTime.Now.Date;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) return NoContent();

                    var totalRows = workSheet.Dimension.Rows;
                    var rowStart = 2;
                    var dt = new DataTable();
                    dt.Columns.AddRange(new[] {
                        new DataColumn("Id", typeof(Guid)),
                        new DataColumn("OvertimeDate", typeof(DateTime)),
                        new DataColumn("OvertimeInAdjust", typeof(DateTime)),
                        new DataColumn("OvertimeOutAdjust", typeof(DateTime)),
                        new DataColumn("OvertimeBreakAdjust", typeof(int))
                    });

                    while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                    {
                        var id = Guid.Parse(workSheet.Cells[rowStart, 1].Value.ToString());
                        var overtimeDate = DateTime.ParseExact(workSheet.Cells[rowStart, 4].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);

                        var overtimeIn = TimeSpan.Parse(workSheet.Cells[rowStart, 5].Value.ToString());
                        var overtimeOut = TimeSpan.Parse(workSheet.Cells[rowStart, 6].Value.ToString());
                        var overtimeInDate = overtimeDate.Add(overtimeIn);
                        var overtimeOutDate = overtimeIn > overtimeOut ? overtimeDate.AddDays(1).Add(overtimeOut) : overtimeDate.Add(overtimeOut);
                        var overtimeBreak = Math.Abs(int.Parse(workSheet.Cells[rowStart, 7].Value.ToString()));

                        var row = dt.NewRow();
                        row.ItemArray = new object[] {
                            id,
                            overtimeDate,
                            overtimeInDate,
                            overtimeOutDate,
                            overtimeBreak
                        };

                        dt.Rows.Add(row);

                        rowStart++;
                    }

                    SpklService.UploadSpklReport(noreg, username, dt);
                }
            }

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class SpklReportController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Core service
        /// </summary>
        protected CoreService CoreService => ServiceProxy.GetService<CoreService>();

        /// <summary>
        /// MDM service
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Approval service
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();

        /// <summary>
        /// SPKL service
        /// </summary>
        protected SpklService SpklService => ServiceProxy.GetService<SpklService>();
        #endregion

        public IActionResult Index()
        {
            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode);
            ViewData["OrganizationInfo"] = orgObj;
            return View();
        }

        public IActionResult DownloadReport(DateTime startDate, DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var keyDate = new DateTime(startDate.Year, startDate.Month, 1);
            var min = ConfigService.GetConfigValue("Spkl.Min", 3);
            var max = ConfigService.GetConfigValue("Spkl.Max", 6);
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure.OrgCode;
            var output = SpklService.GetSpklRequestsByOrganization(orgCode, keyDate, min, max);

            if (!ServiceProxy.UserClaim.Chief)
            {
                output = output.Where(x => x.NoReg == noreg);
            }

            var objectDescriptions = MdmService.GetObjectDescriptions(orgCode, keyDate);

            var title = "Laporan Kerja Lembur - " + keyDate.ToString("dd-MM-yyyy");
            var customSwitches = string.Format(
                                     "--title \"{0}\" " +
                                     "--header-html \"{1}\" " +
                                     "--header-spacing \"0\" " +
                                     "--header-font-size \"9\" ",
                                     title, GetHeaderPdfUrl("/core/default/commonheader?hide=1")
                                 );

            ViewBag.KeyDate = keyDate;

            return new ViewAsPdf("LklCustomReportPdfForm", new Infrastructure.ViewModels.PdfViewModel()
            {
                Object = output,
                ViewData = objectDescriptions,
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

        public IActionResult Download(Guid parentId, string keyDateStr)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var keyDate = DateTime.ParseExact(keyDateStr, "dd/MM/yyyy", CultureInfo.CurrentCulture);
            var documentApproval = ApprovalService.GetDocumentApprovalById(parentId);
            var username = ServiceProxy.UserClaim.Username;
            var output = SpklService.GetCompletedSpklRequestDetails(parentId, keyDate);
            var documentRequestDetail = ApprovalService.GetDocumentRequestDetailViewModel<SpklOvertimeViewModel>(parentId, string.Empty);
            var title = "Laporan Kerja Lembur - " + keyDate.ToString("dd-MM-yyyy");
            var printoutMatrix = CoreService.GetPrintOut(output.FirstOrDefault().DocumentApprovalId.Value);
            var customSwitches = string.Format(
                                     "--title \"{0}\" " +
                                     "--header-html \"{1}\" " +
                                     "--header-spacing \"0\" " +
                                     "--header-font-size \"9\" ",
                                     title, GetHeaderPdfUrl("/core/default/commonheader")
                                 );

            return new ViewAsPdf("LklPdfForm", new Infrastructure.ViewModels.PdfViewModel()
            {
                DocumentApproval = documentApproval,
                Object = documentRequestDetail.Object,
                PrintoutMatrix = printoutMatrix,
                ViewData = output
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