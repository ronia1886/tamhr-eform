using System;
using System.Collections.Generic;
using System.Drawing;
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
    [Route("api/payroll-detail")]
    public class PayrollDetailReportAPIController : ApiControllerBase
    {
        protected PayrollReportService payrollReportService => ServiceProxy.GetService<PayrollReportService>();

        [HttpPost("get-division")]
        public List<DropDownTreeItemModel> GetDivision([FromForm] List<string> DirectorateList)
        {
            var divisions = ServiceProxy.GetService<PayrollReportService>().GetDivisionsFromMultiDirectorate(DirectorateList);

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

        [HttpPost("get-department")]
        public List<DropDownTreeItemModel> GetDepartment([FromForm] List<string> DivisionList)
        {
            var departments = ServiceProxy.GetService<PayrollReportService>().GetDepartmentsFromMultiDivision(DivisionList);

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var department in departments)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", department.OrgCode, department.ObjectText),
                    Text = department.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            return listDropDownTreeItem;
        }

        [HttpPost("get-section")]
        public List<DropDownTreeItemModel> GetSection([FromForm] List<string> DepartmentList)
        {
            var sections = ServiceProxy.GetService<PayrollReportService>().GetSectionsFromMultiDepartment(DepartmentList);

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var section in sections)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", section.OrgCode, section.ObjectText),
                    Text = section.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            return listDropDownTreeItem;
        }

        [HttpPost("get-line")]
        public List<DropDownTreeItemModel> GetLine([FromForm] List<string> SectionList)
        {
            var lines = ServiceProxy.GetService<PayrollReportService>().GetLinesFromMultiSection(SectionList);

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var line in lines)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", line.OrgCode, line.ObjectText),
                    Text = line.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            return listDropDownTreeItem;
        }

        [HttpPost("get-group")]
        public List<DropDownTreeItemModel> GetGroup([FromForm] List<string> LineList)
        {
            var groups = ServiceProxy.GetService<PayrollReportService>().GetGroupsFromMultiLine(LineList);

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var group in groups)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", group.OrgCode, group.ObjectText),
                    Text = group.ObjectText,
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
            return await ServiceProxy.GetService<PayrollReportService>().GetClass().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-reports")]
        public DataSourceResult GetReport([DataSourceRequest] DataSourceRequest request)
        {
            string keyDate = this.Request.Form["keyDate"].ToString();

            DateTime keyDateDT = keyDate != "" ? Convert.ToDateTime(keyDate) : DateTime.Today;

            var getData = payrollReportService.GetPayrollDetailReport(keyDateDT).ToList();

            return getData.ToDataSourceResult(request);
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime keyDate, string employeeName, string listDirectorate, string listDivision, string listDepartment, string listSection, string listLine, string listGroup, string className, string noReg, string type)
        {
            var getData = payrollReportService.GetPayrollDetailReport(keyDate).ToList();

            if (!string.IsNullOrEmpty(listDirectorate) && listDirectorate.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDirectorate.Split(",").Contains(c.Directorate)).ToList();
            }

            if (!string.IsNullOrEmpty(listDivision) && listDivision.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDivision.Split(",").Contains(c.Division)).ToList();
            }

            if (!string.IsNullOrEmpty(listDepartment) && listDepartment.Split(",").Length > 0)
            {
                getData = getData.Where(c => listDepartment.Split(",").Contains(c.Department)).ToList();
            }

            if (!string.IsNullOrEmpty(listSection) && listSection.Split(",").Length > 0)
            {
                getData = getData.Where(c => listSection.Split(",").Contains(c.Section)).ToList();
            }

            if (!string.IsNullOrEmpty(listLine) && listLine.Split(",").Length > 0)
            {
                getData = getData.Where(c => listLine.Split(",").Contains(c.Line)).ToList();
            }

            if (!string.IsNullOrEmpty(listGroup) && listGroup.Split(",").Length > 0)
            {
                getData = getData.Where(c => listGroup.Split(",").Contains(c.Group)).ToList();
            }

            if (!string.IsNullOrEmpty(employeeName))
            {
                getData = getData.Where(c => c.Name.Contains(employeeName)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class.Contains(className)).ToList();
            }

            if (!string.IsNullOrEmpty(noReg))
            {
                getData = getData.Where(c => c.NoReg.Contains(noReg)).ToList();
            }

            if (!string.IsNullOrEmpty(type))
            {
                getData = getData.Where(c => c.OTType.Contains(type)).ToList();
            }

            var fileName = string.Format("TM PAYROLL DETAIL {0:MMM yyyy}_{1:yyyyMMdd}.xlsx", keyDate, DateTime.Now);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    

                    var sheet = package.Workbook.Worksheets.Add("Output");

                    var cols = new[] { "Noreg", "Name", "PostName", "JobName", "Class", "Division", "Department", "Section", "Line", "Group", "Total Working Day", "Total Lost Hour", "Total OT Hour", "OT Type", "Actual Work Hour" };

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

                    int rowIndex = 11;

                    //TITLE
                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[rowIndex-1, i].Value = cols[i - 1];
                        sheet.Cells[rowIndex-1, i].Style.Font.Bold = true;
                        sheet.Cells[rowIndex-1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        sheet.Cells[rowIndex-1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[rowIndex-1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[rowIndex-1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        sheet.Cells[rowIndex-1, i].Style.Font.Color.SetColor(Color.White);
                    }

                    //DATA
                    foreach (var item in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.NoReg;
                        sheet.Cells[rowIndex, 2].Value = item.Name;
                        sheet.Cells[rowIndex, 3].Value = item.PostName;
                        sheet.Cells[rowIndex, 4].Value = item.JobName;
                        sheet.Cells[rowIndex, 5].Value = item.Class;
                        sheet.Cells[rowIndex, 6].Value = item.Division;
                        sheet.Cells[rowIndex, 7].Value = item.Department;
                        sheet.Cells[rowIndex, 8].Value = item.Section;
                        sheet.Cells[rowIndex, 9].Value = item.Line;
                        sheet.Cells[rowIndex, 10].Value = item.Group;
                        sheet.Cells[rowIndex, 11].Value = item.TotalWorkingDay;
                        sheet.Cells[rowIndex, 12].Value = item.LHTotalLostHour;
                        sheet.Cells[rowIndex, 13].Value = item.OTTotalOverTime;
                        sheet.Cells[rowIndex, 14].Value = item.OTType;
                        sheet.Cells[rowIndex, 15].Value = item.ActualWorkHour;

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
    public class PayrollDetailReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            ViewBag.directorateData = GetDirectorateTree();
            return View();
        }

        public List<DropDownTreeItemModel> GetDirectorateTree()
        {
            // Get list of one level hierarchy by noreg, position code, and key date.
            var directorates = ServiceProxy.GetService<PayrollReportService>().GetDirectorates();

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var directorate in directorates)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", directorate.OrgCode, directorate.ObjectText),
                    Text = directorate.ObjectText,
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
