using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Vacation allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.VacationAllowance)]
    public class VacationAllowanceApiController : FormApiControllerBase<VacationAllowanceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<VacationAllowanceViewModel> vacationAllowanceViewModel)
        {
            base.ValidateOnPostCreate(vacationAllowanceViewModel);

            if (vacationAllowanceViewModel.Object == null)
            {
                throw new Exception("Cannot create request because request is empty");
            }

            if (vacationAllowanceViewModel.Object.Departments == null)
            {
                throw new Exception("No participant selected");
            }

            Assert.ThrowIf(vacationAllowanceViewModel.Object.Summaries == null, "Summary cannot be null");
            Assert.ThrowIf(vacationAllowanceViewModel.Object.Summaries.Sum(x => x.Total) == 0, "Total summary cannot be 0");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<VacationAllowanceViewModel> vacationAllowanceViewModel)
        {
            base.ValidateOnPostUpdate(vacationAllowanceViewModel);

            if (vacationAllowanceViewModel.Object == null)
            {
                throw new Exception("Cannot update request because request is empty");
            }

            if (vacationAllowanceViewModel.Object.Departments == null)
            {
                throw new Exception("No participant selected");
            }

            Assert.ThrowIf(vacationAllowanceViewModel.Object.Summaries == null, "Summary cannot be null");
            Assert.ThrowIf(vacationAllowanceViewModel.Object.Summaries.Sum(x => x.Total) == 0, "Total summary cannot be 0");
        }

        private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            return ServiceProxy.GetQueryDataSourceResult<RecreationReward>(@"
SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_RECREATION_REWARD i 
INNER JOIN dbo.TB_R_DOCUMENT_APPROVAL da ON da.Id=i.DocumentApprovalId
LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.BenefitType IN ('recreation', 'vacation') AND i.EventDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
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

            //var output = GetDetailsReport(request, startDate, endDate).Data.Cast<RecreationReward>();
            var output = ClaimBenefitService.GetDocumentRequestDetails<VacationAllowanceViewModel>("vacation-allowance", startDate, endDate);

            var fileName = string.Format("VACATION ALLOWANCE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name", "Location", "Total Ammount", "Event Date", "Bank", "Account Number", "Account Name" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in output)
                    {
                        var item = data.Object;
                        var totalAmount = item.Summaries?.Sum(s => s.Total) ?? 0;
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = data.RequesterName;
                        sheet.Cells[rowIndex, 3].Value = data.Division;
                        sheet.Cells[rowIndex, 4].Value = item.Location;
                        sheet.Cells[rowIndex, 5].Value = totalAmount.ToString("N0");
                        sheet.Cells[rowIndex, 6].Value = item.VacationDate;
                        sheet.Cells[rowIndex, 6].Style.Numberformat.Format = "dd/MM/yyyy";
                        sheet.Cells[rowIndex, 6].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 7].Value = item.AccountType;
                        sheet.Cells[rowIndex, 8].Value = item.AccountNumber;
                        sheet.Cells[rowIndex, 9].Value = item.AccountName;

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

        [HttpPost("update-report")]
        public IActionResult UpdateReport(VacationAllowanceViewModel vacationAllowanceViewModel)
        {
            Guid DocumentApprovalId = vacationAllowanceViewModel.DocumentApprovalId;

            //base.ValidateOnPostUpdate(vacationAllowanceViewModel);

            if (vacationAllowanceViewModel.LocationActual == "")
            {
                throw new Exception("Location cannot be null");
            }

            if (vacationAllowanceViewModel.VacationDateActual == null)
            {
                throw new Exception("Vacation Date cannot be null");
            }

            if (vacationAllowanceViewModel.TotalParticipant == 0)
            {
                throw new Exception("Total Participant cannot 0 value");
            }

            if (vacationAllowanceViewModel.AttachmentFilePath == "")
            {
                throw new Exception("Attachment Report cannot be null");
            }

            ClaimBenefitService.UpdateVacationReport(vacationAllowanceViewModel);

            return NoContent();
        }

    }
    #endregion
}