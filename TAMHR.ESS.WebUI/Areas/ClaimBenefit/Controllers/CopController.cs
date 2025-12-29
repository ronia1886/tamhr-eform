using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// COP API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Cop)]
    [AutoValidateAntiforgeryToken] // ✅ validasi otomatis utk semua non-GET
    public class CopApiController : FormApiControllerBase<CopViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected ClaimBenefitService claimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            claimBenefitService.PreValidateCop(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name);
            claimBenefitService.PreValidateCop(ServiceProxy.UserClaim.NoReg);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<CopViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            string noreg = ServiceProxy.UserClaim.NoReg != requestDetailViewModel.Requester
                ? ServiceProxy.UserClaim.NoReg
                : requestDetailViewModel.Requester;

            claimBenefitService.PreValidatePostCop(noreg, requestDetailViewModel);
        }

        [HttpPost("stnkready")]
        public async Task<IActionResult> StnkReady(string eventName, [FromBody] DocumentRequestDetailViewModel<CopViewModel> documentRequestDetail)
        {
            //update
            //ValidateOnPostUpdate(documentRequestDetail);
            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;
            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail);
            //workflow
            if (!ApprovalService.HasApprovalMatrix(documentRequestDetail.DocumentApprovalId))
            {
                throw new Exception("Approval matrix for this form is not defined. Please contact administrator for more information.");
            }

            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            var doc = ApprovalService.GetDocumentApprovalById(documentRequestDetail.DocumentApprovalId);
            claimBenefitService.CompleteCOPSTNKReady(doc.CreatedBy, documentRequestDetail.Object);

            await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, documentRequestDetail.DocumentApprovalId);

            return NoContent();
        }

        private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            //return ServiceProxy.GetQueryDataSourceResult<CarPurchase>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_CAR_PURCHASE i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
            return ServiceProxy.GetTableValuedDataSourceResult<CarPurchasedReportStoredEntity>(request, new { startDate,endDate });
        }

        [HttpPost("details-report")]
        public DataSourceResult GetDetailsReport(
            [FromForm] DateTime startDate,
            [FromForm] DateTime endDate,
            [DataSourceRequest] DataSourceRequest request)
        {
            return GetDetailsReport(request, startDate, endDate);
        }

        /// ✅ Helper: logic pembuatan file dijadikan satu,
        /// dipakai oleh GET (legacy) & POST (aman)
        private FileContentResult BuildCopReport(DateTime startDate, DateTime endDate)
        {
            var request = new DataSourceRequest();

            var output = GetDetailsReport(request, startDate, endDate).Data.Cast<CarPurchasedReportStoredEntity>();

            var fileName = string.Format("CAR PURCHASE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            using (var package = new ExcelPackage())
            {
                int rowIndex = 2;
                var sheet = package.Workbook.Worksheets.Add("Output");
                var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name","Class","Form Type","Document Number","Submission Type", "Model", "Type", "Color", "Submit On","Buy For" };

                for (var i = 1; i <= cols.Length; i++)
                {
                    sheet.Cells[1, i].Value = cols[i - 1];
                    sheet.Cells[1, i].Style.Font.Bold = true;
                    sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                    foreach (var data in output)
                    {
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.NoReg);
                        sheet.Cells[rowIndex, 2].Value = data.Name;
                        sheet.Cells[rowIndex, 3].Value = data.PostName;
                        sheet.Cells[rowIndex, 4].Value = data.Class;
                        sheet.Cells[rowIndex, 5].Value = data.FormType;
                        sheet.Cells[rowIndex, 6].Value = data.DocumentNumber;
                        sheet.Cells[rowIndex, 7].Value = data.SubmissionType;
                        sheet.Cells[rowIndex, 8].Value = data.CarModel;
                        sheet.Cells[rowIndex, 9].Value = data.CarModelType;
                        sheet.Cells[rowIndex, 10].Value = data.ColorName;
                        sheet.Cells[rowIndex, 11].Value = data.SubmitOn.ToString(format);
                        sheet.Cells[rowIndex, 11].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 12].Value = data.BuyFor;

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    rowIndex++;
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                package.SaveAs(ms);
                return new FileContentResult(
                    ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        /// <summary>
        /// Download reimbursement with date parameter (GET legacy)
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        [HttpGet("download")]
        [Obsolete("Use POST /download with anti-forgery token")]
        public IActionResult Download(DateTime startDate, DateTime endDate)
        {
            return BuildCopReport(startDate, endDate);
        }

        /// <summary>
        /// Download reimbursement (POST aman dengan anti-forgery)
        /// </summary>
        [HttpPost("download")]
        [ValidateAntiForgeryToken]
        public IActionResult DownloadPost([FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            return BuildCopReport(startDate, endDate);
        }
    }
    #endregion
}
