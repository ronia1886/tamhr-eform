using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// KB allowance API controller
    /// </summary>
    [Route("api/" + ApplicationForm.KbAllowance)]
    public class KbAllowanceApiController : FormApiControllerBase<KbAllowanceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Claim benefit service
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion
        //private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        //{
        //    return ServiceProxy.GetQueryDataSourceResult<KBAllowance>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_KB i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
        //}

        //[HttpPost("details-report")]
        //public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        //{
        //    return GetDetailsReport(request, startDate, endDate);
        //}

        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var letterOfGuarantee = ClaimBenefitService.GetDocumentRequestDetails<KbAllowanceViewModel>("kb-allowance", startDate, endDate);

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

            //var output = GetDetailsReport(request, startDate, endDate).Data.Cast<KBAllowance>();
            var reimbursements = ClaimBenefitService.GetDocumentRequestDetails<KbAllowanceViewModel>("kb-allowance", startDate, endDate);

            var fileName = string.Format("KB ALLOWANCE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name", "Family Member Type", "Patient Name", "Hospital", "Hospital Address", "Ammount", "Transaction Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in reimbursements)
                    {
                        var item = data.Object;
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = data.RequesterName;
                        sheet.Cells[rowIndex, 3].Value = data.Division;
                        sheet.Cells[rowIndex, 4].Value = item.FamilyRelationship;
                        sheet.Cells[rowIndex, 5].Value = item.PassienName;
                        sheet.Cells[rowIndex, 6].Value = item.Hospital;
                        sheet.Cells[rowIndex, 7].Value = item.HospitalAddress;
                        sheet.Cells[rowIndex, 8].Value = item.Cost.ToString("N0");
                        sheet.Cells[rowIndex, 9].Value = item.ActionKBDate.HasValue ? item.ActionKBDate.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 9].Style.QuotePrefix = true;

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