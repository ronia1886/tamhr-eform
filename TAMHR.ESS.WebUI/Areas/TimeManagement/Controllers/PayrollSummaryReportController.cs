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
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;


namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    [Route("api/payroll-summary")]
    public class PayrollSummaryAPIController : ApiControllerBase
    {
        protected PayrollReportService payrollReportService => ServiceProxy.GetService<PayrollReportService>();

        [HttpPost("get-reports")]
        public DataSourceResult GetReport([DataSourceRequest] DataSourceRequest request)
        {
            string keyDate = this.Request.Form["keyDate"].ToString();

            DateTime keyDateDT = keyDate != "" ? Convert.ToDateTime(keyDate) : DateTime.Today;

            var getData = payrollReportService.GetPayrollSummaryReport(keyDateDT).ToList();

            return getData.ToDataSourceResult(request);
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(string listDivision, DateTime keyDate)
        {
            var getData = payrollReportService.GetPayrollSummaryReport(keyDate).ToList();

            if (!string.IsNullOrEmpty(listDivision) && listDivision.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDivision.Split(",").Contains(c.Division)).ToList();
            }

            var fileName = string.Format("TM SUMMARY PAYROLL {0:MMM yyyy}_{1:yyyyMMdd}.xlsx", keyDate, DateTime.Now);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {

                    var sheet = package.Workbook.Worksheets.Add("Output");

                    var cols = new[] { "Division", "Total MP", "Total Working Day", "LHTotalMP", "LHTotalOT", "OTTotalMP", "OTTotalOTHour", "Actual Work Hour"};

                    //TAM HEADER
                    sheet.Cells[1, 1].Value = "PT. TOYOTA ASTRA MOTOR";
                    sheet.Cells[1, 1, 1, 3].Merge = true;
                    sheet.Cells[1, 1, 1, 3].Style.Font.Size = 14;

                    sheet.Cells[2, 1].Value = "HUMAN RESOURCE AND GENERAL AFFAIR DIVISION";
                    sheet.Cells[2, 1, 2, 3].Merge = true;
                    sheet.Cells[2, 1, 2, 3].Style.Font.Size = 14;

                    sheet.Cells[4, 1].Value = "TIME MANAGEMENT";
                    sheet.Cells[4, 1, 4, 3].Merge = true;
                    sheet.Cells[4, 1, 4, 3].Style.Font.Size = 16;

                    sheet.Cells[5, 1].Value = "MONTHLY SUMMARY REPORT";
                    sheet.Cells[5, 1, 5, 3].Merge = true;
                    sheet.Cells[5, 1, 5, 3].Style.Font.Size = 16;

                    sheet.Cells[1, 1, 5, 3].Style.Font.Bold = true;

                    //TANGGAL
                    sheet.Cells[1, 7].Value = DateTime.Now.ToString("dd MMM yyyy");
                    sheet.Cells[1, 7].Style.Font.Bold = true;
                    sheet.Cells[1, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    //TTD
                    var HRHierarchies = payrollReportService.GetHRHierarchies(DateTime.Now).ToList()[0];

                    sheet.Cells[2, 4].Value = "APPROVED BY";
                    sheet.Cells[2, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[3, 4].Value = HRHierarchies.JobNameDH;
                    sheet.Cells[3, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[4, 4, 6, 4].Merge = true;
                    sheet.Cells[4, 4, 6, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[7, 4].Value = HRHierarchies.NameDH;
                    sheet.Cells[7, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[8, 4].Value = HRHierarchies.PostNameDH;
                    sheet.Cells[8, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[2, 5].Value = "CHECKED BY";
                    sheet.Cells[2, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[2, 5, 2, 6].Merge = true;
                    sheet.Cells[2, 5, 2, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[3, 5].Value = HRHierarchies.JobNameDpH;
                    sheet.Cells[3, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[4, 5, 6, 5].Merge = true;
                    sheet.Cells[4, 5, 6, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[7, 5].Value = HRHierarchies.NameDpH;
                    sheet.Cells[7, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[8, 5].Value = HRHierarchies.PostNameDpH;
                    sheet.Cells[8, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);


                    sheet.Cells[3, 6].Value = HRHierarchies.JobNameSH;
                    sheet.Cells[3, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[4, 6, 6, 6].Merge = true;
                    sheet.Cells[4, 6, 6, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[7, 6].Value = HRHierarchies.NameSH;
                    sheet.Cells[7, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[8, 6].Value = HRHierarchies.PostNameSH;
                    sheet.Cells[8, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[2, 7].Value = "PREPARED BY";
                    sheet.Cells[2, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[3, 7].Value = "Officer";
                    sheet.Cells[3, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[4, 7, 6, 7].Merge = true;
                    sheet.Cells[4, 7, 6, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[7, 7].Value = ServiceProxy.UserClaim.Name;
                    sheet.Cells[7, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[8, 7].Value = ServiceProxy.UserClaim.PostName;
                    sheet.Cells[8, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);

                    sheet.Cells[2, 4, 8, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[2, 4, 8, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
                    sheet.Cells[2, 4, 8, 7].Style.Font.Bold = true;

                    int rowIndex = 12;

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        if (cols[i - 1].Contains("LH"))
                        {
                            sheet.Cells[rowIndex - 2, i].Value = "Lost Hour";
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Merge = true;
                            sheet.Cells[rowIndex - 1, i].Value = "Total MP";
                            sheet.Cells[rowIndex - 1, i + 1].Value = "Total Lost Hour";

                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.Font.Bold = true;

                            sheet.Cells[rowIndex - 1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 1, i].Style.Font.Bold = true;

                            sheet.Cells[rowIndex - 1, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 1, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 1, i + 1].Style.Font.Bold = true;
                            i++;
                        } else if (cols[i - 1].Contains("OT"))
                        {
                            sheet.Cells[rowIndex - 2, i].Value = "Total OT";
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Merge = true;
                            sheet.Cells[rowIndex - 1, i].Value = "Total MP";
                            sheet.Cells[rowIndex - 1, i + 1].Value = "Total OT Hour";

                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 2, i, rowIndex - 2, i + 1].Style.Font.Bold = true;

                            sheet.Cells[rowIndex - 1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 1, i].Style.Font.Bold = true;

                            sheet.Cells[rowIndex - 1, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex - 1, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowIndex - 1, i + 1].Style.Font.Bold = true;
                            i++;
                        }
                        else
                        {
                            sheet.Cells[rowIndex - 2, i].Value = cols[i - 1];
                            sheet.Cells[rowIndex - 2, i].Style.Font.Bold = true;
                            sheet.Cells[rowIndex - 2, i].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            sheet.Cells[rowIndex - 2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            sheet.Cells[rowIndex - 2, i, rowIndex - 1, i].Merge = true;
                            sheet.Cells[rowIndex - 2, i, rowIndex - 1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                    }

                    foreach (var item in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.Division;
                        sheet.Cells[rowIndex, 2].Value = item.TotalMP;
                        sheet.Cells[rowIndex, 3].Value = item.TotalWorkingDay;
                        sheet.Cells[rowIndex, 4].Value = item.LHTotalMP;
                        sheet.Cells[rowIndex, 5].Value = item.LHTotalLostHour;
                        sheet.Cells[rowIndex, 6].Value = item.OTTotalMP;
                        sheet.Cells[rowIndex, 7].Value = item.OTTotalOvertime;
                        sheet.Cells[rowIndex, 8].Value = item.ActualWorkHour;

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

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class PayrollSummaryReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            ViewBag.divisionData = GetDivisionTree();
            return View();
        }

        public List<DropDownTreeItemModel> GetDivisionTree()
        {
            // Get list of one level hierarchy by noreg, position code, and key date.
            var Divisions = ServiceProxy.GetService<PayrollReportService>().GetDivisions();

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var Division in Divisions)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", Division.OrgCode, Division.ObjectText),
                    Text = Division.ObjectText,
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
