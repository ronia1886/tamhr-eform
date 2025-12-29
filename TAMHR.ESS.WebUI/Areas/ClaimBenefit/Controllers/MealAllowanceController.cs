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
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Meal allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.MealAllowance)]
    //[Permission(PermissionKey.ViewMealAllowance)]
    public class MealAllowanceApiController : FormApiControllerBase<MealAllowanceViewModel>
    {
        protected ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();

        protected override DocumentRequestDetailViewModel<MealAllowanceViewModel> ValidateViewModel(DocumentRequestDetailViewModel<MealAllowanceViewModel> requestDetailViewModel)
        {
            base.ValidateViewModel(requestDetailViewModel);

            var ammountObj = CoreService.GetInfoAllowance(ServiceProxy.UserClaim.NoReg, "mealallowance");
            var ammount = (decimal)ammountObj.GetType().GetProperty("Ammount").GetValue(ammountObj);
            requestDetailViewModel.Object.data.ForEach(x => x.Amount = ammount);
            requestDetailViewModel.Object.AmountTotal = requestDetailViewModel.Object.data.Sum(x => x.Amount);

            requestDetailViewModel.Refresh();

            return requestDetailViewModel;
        }

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            ClaimBenefitService.PreValidateMealAllowance(formKey, ServiceProxy.UserClaim.NoReg);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<MealAllowanceViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object.data == null || requestDetailViewModel.Object.data.Count == 0)
            {
                throw new Exception("Please Input Data Claim.");
            }
        }

        //private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        //{
        //    return ServiceProxy.GetQueryDataSourceResult<MealAllowance>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_MEAL_ALLOWANCE i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.PeriodDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
        //}

        //[HttpPost("details-report")]
        //public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        //{
        //    return GetDetailsReport(request, startDate, endDate);
        //}

        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var letterOfGuarantee = ClaimBenefitService.GetDocumentRequestDetails<MealAllowanceViewModel>("meal-allowance", startDate, endDate);

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

            //var output = GetDetailsReport(request, startDate, endDate).Data.Cast<MealAllowance>();
            var output = ClaimBenefitService.GetDocumentRequestDetails<MealAllowanceViewModel>("meal-allowance", startDate, endDate);

            var fileName = string.Format("MEAL ALLOWANCE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

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

                    foreach (var data in output)
                    {
                        var item = data.Object;
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = data.RequesterName;
                        sheet.Cells[rowIndex, 3].Value = data.Division;
                        sheet.Cells[rowIndex, 4].Value = item.AmountTotal.ToString("N0");
                        sheet.Cells[rowIndex, 5].Value = item.Period;
                        sheet.Cells[rowIndex, 5].Style.Numberformat.Format = "dd/MM/yyyy";
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