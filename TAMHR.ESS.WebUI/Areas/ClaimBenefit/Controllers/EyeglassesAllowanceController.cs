using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Eyeglasses allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.EyeglassesAllowance)]
    public class EyeglassesAllowanceApiController : FormApiControllerBase<EyeglassesAllowanceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        /// <summary>
        /// Pre-validate eyeglassess form
        /// </summary>
        /// <param name="formKey">Form Key</param>
        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            ClaimBenefitService.PreValidateEyeglasess(ServiceProxy.UserClaim.NoReg);
        }

        /// <summary>
        /// Validate on create
        /// </summary>
        /// <param name="requestDetailViewModel">Request Object</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<EyeglassesAllowanceViewModel> requestDetailViewModel)
        {
            ClaimBenefitService.PreValidateEyeglasess(ServiceProxy.UserClaim.NoReg);

            Validate(requestDetailViewModel);
        }

        /// <summary>
        /// Validate on update
        /// </summary>
        /// <param name="requestDetailViewModel">Request Object</param>
        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<EyeglassesAllowanceViewModel> requestDetailViewModel)
        {
            ClaimBenefitService.PreValidateEyeglasess(ServiceProxy.UserClaim.NoReg);

            base.ValidateOnPostUpdate(requestDetailViewModel);

            Validate(requestDetailViewModel);
        }

        private void Validate(DocumentRequestDetailViewModel<EyeglassesAllowanceViewModel> requestDetailViewModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            if (requestDetailViewModel.Object.IsLens)
            {
                var infoLensa = CoreService.GetInfoAllowance(noreg, "EyewearAllowanceType", requestDetailViewModel.Object.LensType ?? "lensa");
                var amountLensa = Convert.ToInt64(infoLensa.GetType().GetProperty("Ammount").GetValue(infoLensa));

                if (requestDetailViewModel.Object.AmountLens > amountLensa)
                {
                    throw new Exception("Cannot input lens ammount more than " + amountLensa.ToString());
                }
            }

            if (requestDetailViewModel.Object.IsFrame)
            {
                var infoFrame = CoreService.GetInfoAllowance(noreg, "EyewearAllowanceType", "frame");
                var amountFrame = Convert.ToInt64(infoFrame.GetType().GetProperty("Ammount").GetValue(infoFrame));

                if (requestDetailViewModel.Object.AmountFrame > amountFrame)
                {
                    throw new Exception("Cannot input frame ammount more than " + amountFrame.ToString());
                }
            }
        }

        private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            return ServiceProxy.GetQueryDataSourceResult<General>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_GENERAL i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.AllowanceType IN ('frame', 'lensa') AND i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
        }

        [HttpPost("details-report")]
        public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            return GetDetailsReport(request, startDate, endDate);
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
            var output = ClaimBenefitService.GetDocumentRequestDetails<EyeglassesAllowanceViewModel>("eyeglasses-allowance", startDate, endDate);

            var fileName = string.Format("EYEGLASSES ALLOWANCE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name", "Lens Type", "Amount Lens", "Amount Frame", "Transaction Date" };

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
                        sheet.Cells[rowIndex, 4].Value = item.LensType;
                        sheet.Cells[rowIndex, 5].Value = item.AmountLens;
                        sheet.Cells[rowIndex, 6].Value = item.AmountFrame;
                        sheet.Cells[rowIndex, 7].Value = data.CreatedOn.ToString(format);
                        sheet.Cells[rowIndex, 7].Style.QuotePrefix = true;

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