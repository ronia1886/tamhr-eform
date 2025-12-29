using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API Controller
    /// <summary>
    /// Lost and return API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.LostAndReturn)]
    [Permission(PermissionKey.CreateLostAndReturn)]
    public class LoastAndReturnApiController : FormApiControllerBase<LostAndReturnViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Field that hold claim benefit service object.
        /// </summary>
        protected ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        #region Override Methods
        protected override void ValidateOnCreate(string formKey) { }
        #endregion

        /// <summary>
        /// Get details report.
        /// </summary>
        /// <param name="startDate">This start date.</param>
        /// <param name="endDate">This end date.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            var reimbursements = ClaimBenefitService.GetDocumentRequestDetails<LostAndReturnViewModel>("lost-and-return", startDate, endDate);

            return await reimbursements.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Download reimbursement with date parameter.
        /// </summary>
        /// <param name="startDate">This start date.</param>
        /// <param name="endDate">This end date.</param>
        [HttpGet("download")]
        public IActionResult Download(DateTime startDate, DateTime endDate)
        {
            var lostAndReturnData = ClaimBenefitService.GetDocumentRequestDetails<LostAndReturnViewModel>("lost-and-return", startDate, endDate);
            var fileName = string.Format("LOST AND RETURN REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Class", "Division", "Document Number", "Document Status", "Progress", "Category", "Document Category", "Lost Date", "Location", "Damaged Remarks" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in lostAndReturnData)
                    {
                        var item = data.Object;

                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = data.RequesterName;
                        sheet.Cells[rowIndex, 3].Value = data.EmployeeSubgroupText;
                        sheet.Cells[rowIndex, 4].Value = data.Division;

                        sheet.Cells[rowIndex, 5].Value = data.DocumentNumber;
                        sheet.Cells[rowIndex, 6].Value = data.DocumentStatusCode;
                        sheet.Cells[rowIndex, 7].Value = data.Progress;
                        sheet.Cells[rowIndex, 8].Value = data.Object.CategoryCode;
                        sheet.Cells[rowIndex, 9].Value = data.Object.DocumentCategoryCode;

                        sheet.Cells[rowIndex, 10].Value = data.SubmitOn.HasValue ? data.SubmitOn.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 10].Style.QuotePrefix = data.SubmitOn.HasValue;
                        sheet.Cells[rowIndex, 11].Value = data.Object.Location;
                        sheet.Cells[rowIndex, 12].Value = data.Object.DamagedRemarks;

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