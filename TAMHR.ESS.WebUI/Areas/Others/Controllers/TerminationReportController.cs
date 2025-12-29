using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API
    [Route("api/termination-report")]
    public class TerminationReportAPIController : ApiControllerBase
    {
        protected TerminationReportService TerminationReportService => ServiceProxy.GetService<TerminationReportService>();

        [HttpPost("gets")]
        public DataSourceResult GetList([DataSourceRequest] DataSourceRequest request)
        {
            string strDateFrom = this.Request.Form["DateFrom"].ToString();

            DateTime DateFrom;
            if (strDateFrom != "")
            {
                DateFrom = Convert.ToDateTime(strDateFrom);
            }
            else
            {
                string tempDateFrom = DateTime.Today.ToString("yyyy-MM-01");
                DateFrom = Convert.ToDateTime(tempDateFrom);

            }

            string strDateTo = this.Request.Form["DateTo"].ToString();
            DateTime DateTo;
            if (strDateTo != "")
            {
                DateTo = Convert.ToDateTime(strDateTo);
            }
            else
            {
                DateTo = DateTime.Today;
            }
            var data = TerminationReportService.GetTerminationReport(DateFrom, DateTo).ToList();
            
            return data.ToDataSourceResult(request);
        }

        //private Dictionary<int, string> userColors = new Dictionary<int, string>() {
        //    {0,"#D4D46A"},
        //    {1,"#5B2971"},
        //    {2,"#246B61"},
        //    {3,"#9717A8"}
        //};

        [HttpPost("termination-report-summary")]
        public IEnumerable<TerminationReportSummaryStoredEntity> GetTerminationReportSummary([FromForm] string divisions,[FromForm] string startDate, [FromForm] string endDate, [FromForm] string category, [FromForm] bool showAll, [FromForm] string emp_class, [FromForm] string documentStatus)
        {
            DateTime convStartDate;
            try
            {
                convStartDate = DateTime.Parse(startDate);
            }
            catch
            {
                convStartDate = DateTime.Parse(startDate.Split('/')[2] + '-' + startDate.Split('/')[1] + '-' + startDate.Split('/')[0]);
            }

            DateTime convEndDate;
            try
            {
                convEndDate = DateTime.Parse(endDate);
            }
            catch
            {
                convEndDate = DateTime.Parse(endDate.Split('/')[2] + '-' + endDate.Split('/')[1] + '-' + endDate.Split('/')[0]);
            }

            if (category == null)
            {
                category = "";
            }
            //var output = await Task.FromResult(ServiceProxy.GetTableValuedSummary<TerminationReportSummaryStoredEntity>("PresenceCode", "Presence", new { startDate = convStartDate, endDate = convEndDate, category, showAll }));
            var output = TerminationReportService.GetTerminationReportSummary(convStartDate, convEndDate,category,showAll,divisions,emp_class,documentStatus).ToList();

            //var viewModel = new List<TerminationReportViewModel>();

            //for (var i = 0; i < output.Count; i++)
            //{
            //    var data = output[i];
            //    var model = new TerminationReportViewModel(data);
                
            //    model.UserColor = userColors[i];
                
            //    viewModel.Add(model);
            //}
            return output;
        }

        [HttpPost("get-division")]
        public List<DropDownTreeItemModel> GetDivision([FromForm] List<string> DirectorateList)
        {
            var divisions = ServiceProxy.GetService<TerminationReportService>().GetDivisionsFromMultiDirectorate(DirectorateList).ToList();

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var division in divisions)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", division.OrgCode, division.ObjectText),
                    Text = division.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            return listDropDownTreeItem;
        }

        [HttpPost("get-class")]
        public async Task<DataSourceResult> GetClass()
        {
            return await TerminationReportService.GetClass().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-document-status")]
        public async Task<DataSourceResult> GetDocumentStatus()
        {
            return await TerminationReportService.GetDocumentStatus().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-termination-type")]
        public async Task<DataSourceResult> GetTerminationType()
        {
            List<Object> tempList = new List<Object>();
            List<GeneralCategory> tempAbsenceList = await Task.Run(() => TerminationReportService.GetTerminationType());
            foreach(GeneralCategory ab in tempAbsenceList)
            {
                dynamic exo = new System.Dynamic.ExpandoObject();
                ((IDictionary<String, Object>)exo).Add("TerminationType", string.Format("{0}", ab.Code));
                ((IDictionary<String, Object>)exo).Add("TerminationName", string.Format("{0}", ab.Name));
                tempList.Add(exo);
            }

            return tempList.ToDataSourceResult(new DataSourceRequest());
        }

        [HttpGet("download")]
        public IActionResult Download(DateTime dateFrom, DateTime dateTo, string listDivision, string className, string terminationType, string documentStatus)
        {
           
            var getData = TerminationReportService.GetTerminationReport(dateFrom, dateTo).ToList();

            if (!string.IsNullOrEmpty(listDivision) && listDivision.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDivision.Split(",").Contains(c.Division)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class.Contains(className)).ToList();
            }

            if (!string.IsNullOrEmpty(terminationType))
            {
                getData = getData.Where(c => c.Terminationtype.Contains(terminationType)).ToList();
            }

            if (!string.IsNullOrEmpty(documentStatus))
            {
                getData = getData.Where(c => c.DocumentStatus.Contains(documentStatus)).ToList();
            }

            var fileName = string.Format("Termination Report {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", dateFrom, dateTo);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Sheet1");

                    var cols = new[] { "Termination Number","NoReg", "Employee Name", "Division","Department",
                        "Section", "Class", "Job Name", "Termination Type", "Reason", 
                        "Submission Date", "End Date","Status"};

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    sheet.Column(1).Width = 20;
                    sheet.Column(2).Width = 8;
                    sheet.Column(3).Width = 21.5;
                    sheet.Column(4).Width = 21.5;
                    sheet.Column(5).Width = 21.5;
                    sheet.Column(6).Width = 21.5;
                    sheet.Column(7).Width = 8;
                    sheet.Column(8).Width = 15;
                    sheet.Column(9).Width = 16;
                    sheet.Column(10).Width = 25;
                    sheet.Column(11).Width = 16;
                    sheet.Column(12).Width = 16;
                    sheet.Column(13).Width = 12;
                    foreach (var data in getData)
                    {
                        
                        sheet.Cells[rowIndex, 1].Value = data.DocumentNumber;
                        sheet.Cells[rowIndex, 2].Value = data.NoReg;
                        sheet.Cells[rowIndex, 3].Value = data.Name;
                        sheet.Cells[rowIndex, 4].Value = data.Division;
                        sheet.Cells[rowIndex, 5].Value = data.Department;
                        sheet.Cells[rowIndex, 6].Value = data.Section;
                        sheet.Cells[rowIndex, 7].Value = data.Class;
                        sheet.Cells[rowIndex, 8].Value = data.JobName;
                        sheet.Cells[rowIndex, 9].Value = data.TerminationName;
                        sheet.Cells[rowIndex, 10].Value = data.Reason;
                        sheet.Cells[rowIndex, 11].Value = data.SubmissionDate.ToString("dd MMMM yyyy");
                        sheet.Cells[rowIndex, 12].Value = data.EndDate.ToString("dd MMMM yyyy");
                        sheet.Cells[rowIndex, 13].Value = data.DocumentStatus;

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
        
        [HttpGet("download-pdf")]
        public IActionResult DownloadPdf(DateTime dateFrom, DateTime dateTo, string listDivision, string className, string terminationType, string documentStatus)
        {
            var getData = TerminationReportService.GetTerminationReport(dateFrom, dateTo).ToList();

            if (!string.IsNullOrEmpty(listDivision) && listDivision.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDivision.Split(",").Contains(c.Division)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class.Contains(className)).ToList();
            }

            if (!string.IsNullOrEmpty(terminationType))
            {
                getData = getData.Where(c => c.Terminationtype.Contains(terminationType)).ToList();
            }

            if (!string.IsNullOrEmpty(documentStatus))
            {
                getData = getData.Where(c => c.DocumentStatus.Contains(documentStatus)).ToList();
            }

            var title = string.Format("Termination Report {0:ddMMyyyy}-{1:ddMMyyyy}", dateFrom, dateTo);

            var customSwitches = string.Format("--title \"{0}\" ", title);

            var objFilter = new TerminationReportFilter();
            objFilter.DateFrom = dateFrom;
            objFilter.DateTo = dateTo;
            return new ViewAsPdf("~/Areas/Others/Views/TerminationReport/Pdf/TerminationReportPdf.cshtml", new PdfViewModel()
            {
                Object = getData,
                ViewData = objFilter,
            })
            {
                CustomSwitches = customSwitches,
                ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                FileName = title + ".pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 25, 10, 25)
            };
        }

        [Route("update-inline")]
        [HttpPost]
        public IActionResult EditingInline_Update()
        {
            var Id = new Guid(Request.Form["Id"]);
            var endDate = DateTime.Parse(Request.Form["EndDate"]);
            var noreg = ServiceProxy.UserClaim.NoReg;

            TerminationReportService.UpdateTerminationDate(Id, endDate, noreg);

            return Ok();
        }


    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.Others)]
    public class TerminationReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            ViewBag.divisionData = GetDivisionTree();
            return View();
        }

        public List<DropDownTreeItemModel> GetDivisionTree()
        {
            // Get list of one level hierarchy by noreg, position code, and key date.
            var divisions = ServiceProxy.GetService<TerminationReportService>().GetDivisions();

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var division in divisions)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", division.OrgCode, division.ObjectText),
                    Text = division.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            // Return partial view with given view model.
            return listDropDownTreeItem;
        }


    }
    #endregion
}
