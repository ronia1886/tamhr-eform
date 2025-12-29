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
    #region API
    [Route("api/daily-reconciliation")]
    public class DailyReconciliationReportAPIController : ApiControllerBase
    {
        protected DailyReconciliationService dailyReconciliationService => ServiceProxy.GetService<DailyReconciliationService>();
        protected UserService userService => ServiceProxy.GetService<UserService>();

        [HttpPost("gets")]
        public DataSourceResult GetList([DataSourceRequest] DataSourceRequest request)
        {
            string employeeName = this.Request.Form["employeeName"].ToString();

            string strDateFrom = this.Request.Form["DateFrom"].ToString();

            DateTime DateFrom;
            if (strDateFrom != "")
            {
                DateFrom = Convert.ToDateTime(strDateFrom);
            }
            else
            {
                DateFrom = DateTime.Today;
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
            var data = dailyReconciliationService.GetDailyReconciliation(employeeName, DateFrom, DateTo)
                .ToList();
            
            return data.ToDataSourceResult(request);
        }

        [HttpPost("absence-summary")]
        public IEnumerable<DailyReconciliationSummaryStoredEntity> GetAbsenceSummary([FromForm]string employeeName, [FromForm] string startDate, [FromForm] string endDate,
            [FromForm] string[] listDirectorate, [FromForm] string[] listDivision, [FromForm] string[] listDepartment, [FromForm] string[] listSection, [FromForm] string[] listLine, [FromForm] string[] listGroup, [FromForm] string className, [FromForm] string absentStatus
            /*[FromForm] string category, [FromForm] bool showAll*/, [DataSourceRequest] DataSourceRequest request)
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
            var getData = dailyReconciliationService.GetDailyReconciliation("", convStartDate, convEndDate).ToList();

            if (!string.IsNullOrEmpty(employeeName))
            {
                getData = getData.Where(c => c.Name == employeeName).ToList();
            }

            if (listDirectorate.Length > 0)
            {
                getData = getData.Where(c => listDirectorate.Contains(c.Directorate)).ToList();
            }

            if (listDivision.Length > 0)
            {
                getData = getData.Where(c => listDivision.Contains(c.Division)).ToList();
            }

            if (listDepartment.Length > 0)
            {
                getData = getData.Where(c => listDepartment.Contains(c.Department)).ToList();
            }

            if (listSection.Length > 0)
            {
                getData = getData.Where(c => listSection.Contains(c.Section)).ToList();
            }

            if (listLine.Length > 0)
            {
                getData = getData.Where(c => listLine.Contains(c.Line)).ToList();
            }

            if (listGroup.Length > 0)
            {
                getData = getData.Where(c => listGroup.Contains(c.Group)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class == className).ToList();
            }

            if (!string.IsNullOrEmpty(absentStatus))
            {
                getData = getData.Where(c => c.AbsentStatus == absentStatus).ToList();
            }

            var output = getData.GroupBy(x => new { x.AbsentStatus, x.AbsentName }).Select(group => new DailyReconciliationSummaryStoredEntity
            {
                Id = Guid.NewGuid(),
                PresenceCode = Convert.ToInt32(group.Key.AbsentStatus),
                Name = group.Key.AbsentName,
                Total = group.Count()

            });
            //var output = await Task.FromResult(ServiceProxy.GetTableValuedSummary<DailyReconciliationSummaryStoredEntity>("PresenceCode", "Presence", new { startDate = convStartDate, endDate = convEndDate, category, showAll }));
            //var output = dailyReconciliationService.GetDailyReconciliationSummary(convStartDate, convEndDate);
            //var output = ServiceProxy.GetTableValuedSummary<DailyReconciliationSummaryStoredEntity>("Name", "Mangkir", new { startDate = convStartDate, endDate = convEndDate, category, showAll });
            return output;
        }

        [HttpPost("get-division")]
        public List<DropDownTreeItemModel> GetDivision([FromForm] List<string> DirectorateList)
        {
            var divisions = ServiceProxy.GetService<DailyReconciliationService>().GetDivisionsFromMultiDirectorate(DirectorateList).ToList();

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
            var departments = ServiceProxy.GetService<DailyReconciliationService>().GetDepartmentsFromMultiDivision(DivisionList).ToList();

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
            var sections = ServiceProxy.GetService<DailyReconciliationService>().GetSectionsFromMultiDepartment(DepartmentList).ToList();

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
            var lines = ServiceProxy.GetService<DailyReconciliationService>().GetLinesFromMultiSection(SectionList).ToList();

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
            var groups = ServiceProxy.GetService<DailyReconciliationService>().GetGroupsFromMultiLine(LineList).ToList();

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
            return await dailyReconciliationService.GetClass().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-presence-code")]
        public async Task<DataSourceResult> GetPresenceCode()
        {
            List<Object> tempList = new List<Object>();
            List<Absence> tempAbsenceList = await Task.Run(() => dailyReconciliationService.GetPresenceCode());
            foreach(Absence ab in tempAbsenceList)
            {
                dynamic exo = new System.Dynamic.ExpandoObject();
                ((IDictionary<String, Object>)exo).Add("CodePresensi", string.Format("{0}", ab.CodePresensi));
                ((IDictionary<String, Object>)exo).Add("ObjectText", string.Format("{0} - {1}", ab.CodePresensi, ab.Name));
                tempList.Add(exo);
            }

            return tempList.ToDataSourceResult(new DataSourceRequest());
        }

        [HttpGet("download")]
        public IActionResult Download(string employeeName, DateTime dateFrom, DateTime dateTo, string listDirectorate, string listDivision, string listDepartment, string listSection, string listLine, string listGroup,string className, string absentStatus, string noReg)
        {
            if (employeeName == null)
            {
                employeeName = "";
            }
            var getData = dailyReconciliationService.GetDailyReconciliation(employeeName,dateFrom, dateTo).ToList();

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
                getData = getData.Where(c => c.Class==className).ToList();
            }

            if (!string.IsNullOrEmpty(absentStatus))
            {
                getData = getData.Where(c => c.AbsentStatus==absentStatus).ToList();
            }

            if (!string.IsNullOrEmpty(noReg))
            {
                getData = getData.Where(c => c.NoReg==noReg).ToList();
            }

            var fileName = string.Format("Daily Reconciliation Report {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", dateFrom, dateTo);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Sheet1");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "Noreg", "Name", "PostName", "JobName", "Class", "Division", "Department", "Section", "Line", "Group", "Working Date", "Proxy In", "Proxy Out", "Presence Code", "Reason"};

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        sheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                    }
                    int no = 1;
                    foreach (var data in getData)
                    {
                        string strTimeIn = "";
                        if (data.WorkingTimeIn != null)
                        {
                            strTimeIn = data.WorkingTimeIn.Value.ToString("HH:mm:ss");
                        }
                        string strTimeOut = "";
                        if (data.WorkingTimeOut != null)
                        {
                            strTimeOut = data.WorkingTimeOut.Value.ToString("HH:mm:ss");
                        }
                        sheet.Cells[rowIndex, 1].Value = data.NoReg;
                        sheet.Cells[rowIndex, 2].Value = data.Name;
                        sheet.Cells[rowIndex, 3].Value = data.PostName;
                        sheet.Cells[rowIndex, 4].Value = data.JobName;
                        sheet.Cells[rowIndex, 5].Value = data.Class;
                        sheet.Cells[rowIndex, 6].Value = data.Division;
                        sheet.Cells[rowIndex, 7].Value = data.Department;
                        sheet.Cells[rowIndex, 8].Value = data.Section;
                        sheet.Cells[rowIndex, 9].Value = data.Line;
                        sheet.Cells[rowIndex, 10].Value = data.Group;
                        sheet.Cells[rowIndex, 11].Value = data.WorkingDate.ToString(format);
                        sheet.Cells[rowIndex, 12].Value = strTimeIn;
                        sheet.Cells[rowIndex, 13].Value = strTimeOut;
                        sheet.Cells[rowIndex, 14].Value = data.AbsentStatus;
                        sheet.Cells[rowIndex, 15].Value = data.AbsentName;
                        
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

        [HttpPost("getactives")]
        public async Task<DataSourceResult> GetActiveUsers([DataSourceRequest] DataSourceRequest request)
        {
            return await userService.GetActiveUsers().ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class DailyReconciliationReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            ViewBag.directorateData = GetDirectorateTree();
            return View();
        }

        public List<DropDownTreeItemModel> GetDirectorateTree()
        {
            // Get list of one level hierarchy by noreg, position code, and key date.
            var directorates = ServiceProxy.GetService<DailyReconciliationService>().GetDirectorates();

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
