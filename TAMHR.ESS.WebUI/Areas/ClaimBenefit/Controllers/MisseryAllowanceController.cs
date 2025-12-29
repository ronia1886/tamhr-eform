using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Condolance allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.CondolanceAllowance)]
    public class MisseryAllowanceApiController : FormApiControllerBase<MisseryAllowanceViewModel>
    {
        /// <summary>
        /// Claim benefit service
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        //private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        //{
        //    return ServiceProxy.GetQueryDataSourceResult<General>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_GENERAL i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.AllowanceType IN ('miseryallowance') AND i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
        //}

        //[HttpPost("details-report")]
        //public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        //{
        //    return GetDetailsReport(request, startDate, endDate);
        //}

        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var letterOfGuarantee = ClaimBenefitService.GetDocumentRequestDetails<MisseryAllowanceViewModel>("condolance-allowance", startDate, endDate);

            return await letterOfGuarantee.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Download reimbursement with date parameter
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        [HttpGet("download")]
        public IActionResult Download(DateTime startDate, DateTime endDate)
        {
            var request = new DataSourceRequest();

            //var output = GetDetailsReport(request, startDate, endDate).Data.Cast<General>();
            var missery = ClaimBenefitService.GetDocumentRequestDetails<MisseryAllowanceViewModel>("condolance-allowance", startDate, endDate);

            var fileName = string.Format("CONDOLANCE ALLOWANCE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name", "Ammount", "Transaction Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in missery)
                    {
                        var item = data.Object;
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = HttpUtility.HtmlEncode(data.RequesterName);
                        sheet.Cells[rowIndex, 3].Value = data.Division;
                        sheet.Cells[rowIndex, 4].Value = HttpUtility.HtmlEncode(item.AllowancesAmount.ToString("N0"));
                        sheet.Cells[rowIndex, 5].Value = item.MisseryDate;
                        sheet.Cells[rowIndex, 5].Style.QuotePrefix = true;

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