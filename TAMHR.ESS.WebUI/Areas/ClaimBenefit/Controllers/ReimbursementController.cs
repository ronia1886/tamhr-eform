using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using OfficeOpenXml;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Reimbursement API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Reimbursement)]
    [ApiController]
    public class ReimbursementApiController : FormApiControllerBase<ReimbursementViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Claim benefit service
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        /// <summary>
        /// Validate on post create
        /// </summary>
        /// <param name="reimbursementViewModel">Reimbursement Object</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<ReimbursementViewModel> reimbursementViewModel)
        {
            base.ValidateOnPostCreate(reimbursementViewModel);
        }

        /// <summary>
        /// Validate on post update
        /// </summary>
        /// <param name="reimbursementViewModel">Reimbursement Object</param>
        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<ReimbursementViewModel> reimbursementViewModel)
        {
            base.ValidateOnPostUpdate(reimbursementViewModel);
        }

        /// <summary>
        /// Update total claim insurance and company
        /// </summary>
        /// <param name="point">Point Object</param>
        [HttpPost("update-point")]
        public IActionResult UpdatePoint(ReimbursementPointViewModel point)
        {
            ClaimBenefitService.UpdateReimbursementPoint(point);

            return NoContent();
        }

        /// <summary>
        /// Get details report
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="request">Data Source Request Object</param>
        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            var reimbursements = ClaimBenefitService.GetDocumentRequestDetails<ReimbursementViewModel>("reimbursement", startDate, endDate);

            return await reimbursements.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Download reimbursement with date parameter
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        [HttpGet("download")]
        public IActionResult Download(DateTime startDate, DateTime endDate)
        {
            var reimbursements = ClaimBenefitService.GetDocumentRequestDetails<ReimbursementViewModel>("reimbursement", startDate, endDate);
            var fileName = string.Format("REIMBURSEMENT REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Class", "Division", "Document Number", "Document Status", "Progress", "Family Relationship", "Patient Name", "Birth Date", "Ext./Phone Number", "Entry Date", "Out Date", "Hospital", "Hospital Address", "Cost", "Insurance Claim", "Company Claim", "Account Name", "Account Number", "Bank", "Reimbursement Type", "Submit Date", "Approve Date" };

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
                        sheet.Cells[rowIndex, 3].Value = data.EmployeeSubgroupText;
                        sheet.Cells[rowIndex, 4].Value = data.Division;

                        sheet.Cells[rowIndex, 5].Value = data.DocumentNumber;
                        sheet.Cells[rowIndex, 6].Value = data.DocumentStatusCode;
                        sheet.Cells[rowIndex, 7].Value = data.Progress;
                        sheet.Cells[rowIndex, 8].Value = item.FamilyRelationship == "rsanak" ? "Anak" : (item.FamilyRelationship == "rspasangan" ? "Pasangan" : "Karyawan");
                        sheet.Cells[rowIndex, 9].Value = item.FamilyRelationship == "rsanak" ? item.PatientChildName : item.PatientName;
                        sheet.Cells[rowIndex, 10].Value = item.BirthDate.HasValue ? item.BirthDate.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 10].Style.QuotePrefix = item.BirthDate.HasValue;
                        sheet.Cells[rowIndex, 11].Value = item.PhoneNumber;
                        sheet.Cells[rowIndex, 12].Value = item.DateOfEntry.HasValue ? item.DateOfEntry.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 12].Style.QuotePrefix = item.DateOfEntry.HasValue;
                        sheet.Cells[rowIndex, 13].Value = item.DateOfOut.HasValue ? item.DateOfOut.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 13].Style.QuotePrefix = item.DateOfOut.HasValue;
                        sheet.Cells[rowIndex, 14].Value = item.IsOtherHospital ? item.OtherHospital : item.Hospital;
                        sheet.Cells[rowIndex, 15].Value = item.HospitalAddress;
                        sheet.Cells[rowIndex, 16].Value = item.Cost.ToString("N0");
                        sheet.Cells[rowIndex, 17].Value = item.TotalClaim.ToString("N0");
                        sheet.Cells[rowIndex, 18].Value = item.TotalCompanyClaim.ToString("N0");
                        sheet.Cells[rowIndex, 19].Value = item.AccountName;
                        sheet.Cells[rowIndex, 20].Value = item.AccountNumber;
                        sheet.Cells[rowIndex, 21].Value = item.BankCode;
                        sheet.Cells[rowIndex, 22].Value = item.InPatient;

                        sheet.Cells[rowIndex, 23].Value = data.SubmitOn.HasValue ? data.SubmitOn.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 23].Style.QuotePrefix = data.SubmitOn.HasValue;

                        sheet.Cells[rowIndex, 24].Value = data.LastApprovedOn.HasValue && data.DocumentStatusCode == DocumentStatus.Completed ? data.LastApprovedOn.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 24].Style.QuotePrefix = data.LastApprovedOn.HasValue && data.DocumentStatusCode == DocumentStatus.Completed;

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

        /// <summary>
        /// Validate object before create or update
        /// </summary>
        /// <param name="reimbursementViewModel">Reimbursement Object</param>
        protected override DocumentRequestDetailViewModel<ReimbursementViewModel> ValidateViewModel(DocumentRequestDetailViewModel<ReimbursementViewModel> reimbursementViewModel)
        {
            Assert.ThrowIf(reimbursementViewModel.Object == null, "Cannot create request because request is empty");
            Assert.ThrowIf(reimbursementViewModel.Object.DateOfEntry > reimbursementViewModel.Object.DateOfOut, "Tanggal Masuk tidak dapat lebih besar dari Tanggal Keluar");

            var documentApproval = GetDocumentApproval(reimbursementViewModel.DocumentApprovalId);
            var creator = documentApproval != null ? documentApproval.CreatedBy : ServiceProxy.UserClaim.NoReg;
            var keyDate = documentApproval != null ? documentApproval.CreatedOn : DateTime.Now;
            var bank = ClaimBenefitService.GetDataBank(creator, keyDate.Date).FirstOrDefault();

            Assert.ThrowIf(bank == null, string.Format("Account number is not registered"));

            reimbursementViewModel.Object.AccountType = "rekening";
            reimbursementViewModel.Object.AccountName = bank.AccountName ?? bank.Name;
            reimbursementViewModel.Object.AccountNumber = bank.AccountNumber;
            reimbursementViewModel.Object.BankCode = bank.BankName;

            reimbursementViewModel.Refresh();

            return reimbursementViewModel;
        }
    }
    #endregion
}