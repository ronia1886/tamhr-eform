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
using Newtonsoft.Json;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Concept idea allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ConceptIdeaAllowance)]
    public class ConceptIdeaApiController : FormApiControllerBase<IdeBerkonsepViewModel>
    {
        protected ClaimBenefitQueryService ClaimBenefitServicePartial => ServiceProxy.GetService<ClaimBenefitQueryService>();

        protected override void ValidateOnCreate(string formKey) { }

        protected override DocumentRequestDetailViewModel<IdeBerkonsepViewModel> ValidateViewModel(DocumentRequestDetailViewModel<IdeBerkonsepViewModel> requestDetailViewModel)
        {
            base.ValidateViewModel(requestDetailViewModel);

            var value = int.Parse(requestDetailViewModel.Object.Value);

            var objAmountInfo = ClaimBenefitServicePartial.GetInfoAmmountConceptIdea(value, "IdeaConceptMatrix").FirstOrDefault();

            Assert.ThrowIf(objAmountInfo == null, $"Score with value <b>{value}</b> is not registered in matrix");

            requestDetailViewModel.Object.Amount = Convert.ToInt64(objAmountInfo.GetType().GetProperty("Ammount").GetValue(objAmountInfo));

            requestDetailViewModel.ObjectValue = JsonConvert.SerializeObject(requestDetailViewModel.Object);

            return requestDetailViewModel;
        }

        [HttpPost("details-report")]
        public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            return ServiceProxy.GetQueryDataSourceResult<ConceptIdea>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_CONCEPT_IDEA i LEFT JOIN dbo.MDM_ACTUAL_ORGANIZATION_STRUCTURE u ON u.NoReg = i.NoReg and u.Staffing = 100 WHERE i.CreatedOn BETWEEN @startDate AND @endDate ", request, new { startDate, endDate });
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

            // var output = ServiceProxy.GetQueryDataSourceResult<ConceptIdea>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_CONCEPT_IDEA i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate }).Data.Cast<ConceptIdea>();

            //var output = ServiceProxy.GetQueryDataSourceResult<ConceptIdeaView>(@"SELECT * from VW_CONCEPT_IDEA Where CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate }).Data.Cast<ConceptIdeaView>();
            var dataSourceResult = ClaimBenefitServicePartial.GetConceptIdeaReport(request, startDate, endDate);
            var output = dataSourceResult.Data.OfType<ConceptIdeaView>();

            var fileName = string.Format("CONCEPT IDEA REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd-MMM-yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name","Job Name","Class","Directorate","Division","Department","Section","Line","Group","Superior", "Title", "Point", "Ammount", "Last Approved" };

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
                        sheet.Cells[rowIndex, 4].Value = data.JobName;
                        sheet.Cells[rowIndex, 5].Value = data.Class;
                        sheet.Cells[rowIndex, 6].Value = data.Directorate;
                        sheet.Cells[rowIndex, 7].Value = data.Division;
                        sheet.Cells[rowIndex, 8].Value = data.Department;
                        sheet.Cells[rowIndex, 9].Value = data.Section;
                        sheet.Cells[rowIndex, 10].Value = data.Line;
                        sheet.Cells[rowIndex, 11].Value = data.Group;
                        sheet.Cells[rowIndex, 12].Value = data.Superior;
                        sheet.Cells[rowIndex, 13].Value = data.Title;
                        sheet.Cells[rowIndex, 14].Value = int.Parse(data.Point);
                        sheet.Cells[rowIndex, 15].Value = data.Ammount.ToString("N0");
                        sheet.Cells[rowIndex, 16].Value = data.CreatedOn.ToString(format);
                        sheet.Cells[rowIndex, 16].Style.QuotePrefix = true;

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