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
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;


namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    [Route("api/absence-log-tracking")]
    public class AbsenceLogTrackingAPIController : FormApiControllerBase<AbsenceLogTrackingStoredEntity>
    {
        protected LogTrackingService logTrackingService => ServiceProxy.GetService<LogTrackingService>();

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
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string createdDate = this.Request.Form["createdDate"].ToString();

            DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : DateTime.Today;
            DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : DateTime.Today;
            DateTime? createdDateDT = createdDate != "" ? Convert.ToDateTime(createdDate) : DateTime.MinValue;

            if(createdDateDT == DateTime.MinValue)
            {
                createdDateDT = null;
            }

            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;

            var getData = logTrackingService.GetAbsenceLogTracking(startDateDT, endDateDT, NoReg, PostCode, createdDateDT).ToList();

            return getData.ToDataSourceResult(request);
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime startDate, DateTime endDate, string employeeName, string listDirectorate, string listDivision, string listDepartment, string listSection, string listLine, string listGroup, string className, string presenceCode, string createdBy, string noReg, DateTime? createdDate)
        {
            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;

            var getData = logTrackingService.GetAbsenceLogTracking(startDate, endDate, NoReg, PostCode, createdDate).ToList();

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

            if (!string.IsNullOrEmpty(presenceCode))
            {
                getData = getData.Where(c => c.AbsentStatus == presenceCode).ToList();
            }

            if (!string.IsNullOrEmpty(createdBy))
            {
                getData = getData.Where(c => c.CreatedName.Contains(createdBy)).ToList();
            }

            if (!string.IsNullOrEmpty(noReg))
            {
                getData = getData.Where(c => c.NoReg.Contains(noReg)).ToList();
            }

            var fileName = string.Format("TM PROXY TIME LOG {0:ddMMyyyy}-{1:ddMMyyyy}_{2:yyyyMMdd}.xlsx", startDate, endDate, DateTime.Now);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var sheet = package.Workbook.Worksheets.Add("Output");

                    var cols = new[] { "Noreg", "Name", "PostName", "JobName", "Class", "Division", "Department", "Section", "Line", "Group", "Proxy In", "Proxy Out", "Presence Code", "Proxy In-After", "Proxy Out-After", "Presence Code-After", "Created Date", "Created By" };

                    //var now = DateTime.Now;
                    //
                    //sheet.Cells[1, cols.Length].Value = now.

                    int rowIndex = 2;

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
                        sheet.Cells[rowIndex, 11].Value = item.ProxyIn.HasValue ? item.ProxyIn.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 12].Value = item.ProxyOut.HasValue ? item.ProxyOut.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 13].Value = item.AbsentName + " - " + item.AbsentStatus;
                        sheet.Cells[rowIndex, 14].Value = item.MemoProxyIn.HasValue ? item.MemoProxyIn.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 15].Value = item.MemoProxyOut.HasValue ? item.MemoProxyOut.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 16].Value = item.MemoAbsentName + " - " + item.MemoAbsentStatus;
                        sheet.Cells[rowIndex, 17].Value = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("dd/MM/yyyy") : "-";
                        sheet.Cells[rowIndex, 18].Value = item.CreatedName;

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

        [HttpPost("get-presence-code")]
        public async Task<DataSourceResult> GetPresenceCode()
        {
            List<Object> tempList = new List<Object>();
            List<Absence> tempAbsenceList = await Task.Run(() => logTrackingService.GetPresenceCode());
            foreach (Absence ab in tempAbsenceList)
            {
                dynamic exo = new System.Dynamic.ExpandoObject();
                ((IDictionary<String, Object>)exo).Add("CodePresensi", string.Format("{0}", ab.CodePresensi));
                ((IDictionary<String, Object>)exo).Add("ObjectText", string.Format("{0} - {1}", ab.CodePresensi, ab.Name));
                tempList.Add(exo);
            }

            return tempList.ToDataSourceResult(new DataSourceRequest());
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class AbsenceLogTrackingController : MvcControllerBase
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
