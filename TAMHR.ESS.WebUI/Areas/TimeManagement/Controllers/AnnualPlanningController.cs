using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    public class EmployeeHierarcy
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
    }

    #region API Controller
    [Route("api/annual-planning")]
    //[Permission(PermissionKey.ReportAnnualPlanning)]
    public class AnnualPlanningAPIController : ApiControllerBase
    {
        protected AnnualPlanningService annualPlanningService => ServiceProxy.GetService<AnnualPlanningService>();
        protected FormService formService => ServiceProxy.GetService<FormService>();
        protected MdmService mdmService => ServiceProxy.GetService<MdmService>();
        protected ConfigService configService => ServiceProxy.GetService<ConfigService>();

        [HttpPost("gets")]
        public DataSourceResult GetList([DataSourceRequest] DataSourceRequest request)
        {
            //var NoReg = this.Request.Form["NoReg"].ToString();
            var NoReg = Request.Query["NoReg"].ToString();
            var strPeriod = Request.Query["Period"].ToString();
            //var strPeriod = this.Request.Form["Period"].ToString();
            var PostCode = Request.Query["PostCode"].ToString();

            int Period= DateTime.Now.Month != 12 ? DateTime.Now.Year : DateTime.Now.Year + 1;
            if (strPeriod != "")
            {
                Period = Convert.ToInt32(strPeriod);
            }
            //var Period = Convert.ToInt32(Request.Query["YearPeriod"].ToString());
            //int Period = DateTime.Now.Year+1;
            if (NoReg == "")
            {
                NoReg = ServiceProxy.UserClaim.NoReg;
            }

            if (PostCode == "")
            {
                PostCode = ServiceProxy.UserClaim.PostCode;
            }

            var getDataPlanningSummary = annualPlanningService.GetAnnualPlanningSummary(Period,NoReg, PostCode).ToList();

            return getDataPlanningSummary.ToDataSourceResult(request);
        }

        [HttpPost("get-reports")]
        public DataSourceResult GetReport([DataSourceRequest] DataSourceRequest request)
        {
            string strDateFrom = this.Request.Form["DateFrom"].ToString();
            string strDateTo = this.Request.Form["DateTo"].ToString();

            DateTime DateFrom = strDateFrom != "" ? Convert.ToDateTime(strDateFrom) : DateTime.Today;
            DateTime DateTo = strDateTo != "" ? Convert.ToDateTime(strDateTo) : DateTime.Today;

            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;
            var getData = annualPlanningService.GetAnnualPlanningReport(DateFrom, DateTo, NoReg, PostCode);

            var distinctNoReg = getData.Select(x => x.NoReg).Distinct().ToList();

            var result = getData.ToDataSourceResult(request);

            result.Total = distinctNoReg.Count();

            return result;
        }

        [HttpPost("get-leave-details")]
        public DataSourceResult GetLeaveDetailList([DataSourceRequest] DataSourceRequest request,int Period, string NoReg)
        {
            var getDataPlanningDetailSummary = annualPlanningService.GetAnnualLeavePlanningDetailSummary(Period, NoReg)
                .OrderBy(ob => ob.PlanDate).ToList();
            
            return getDataPlanningDetailSummary.ToDataSourceResult(request);
        }

        [HttpPost("get-wfh-details")]
        public DataSourceResult GetWFHDetailList([DataSourceRequest] DataSourceRequest request, int Period, string NoReg)
        {
            
            if (request.Filters.Count == 0)
            {
                string strDateFrom = this.Request.Form["DateFrom"].ToString();
                string strDateTo = this.Request.Form["DateTo"].ToString();
                DateTime dateFrom = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-01");
                DateTime dateTo = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
                if (strDateFrom != "")
                {
                    dateFrom = Convert.ToDateTime(strDateFrom);
                }

                if (strDateTo != "")
                {
                    dateTo = Convert.ToDateTime(strDateTo);
                }
                
                request.Filters = new List<IFilterDescriptor>();
                request.Filters.Add(new FilterDescriptor("PlanDate", FilterOperator.IsGreaterThanOrEqualTo, dateFrom));
                request.Filters.Add(new FilterDescriptor("PlanDate", FilterOperator.IsLessThanOrEqualTo, dateTo));
            }

            var getDataPlanningDetailSummary = annualPlanningService.GetAnnualWFHPlanningDetailSummary(Period, NoReg)
                .OrderBy(ob=> ob.PlanDate).ToList();

            return getDataPlanningDetailSummary.ToDataSourceResult(request);
        }

        [HttpPost("get-bdjk-details")]
        public DataSourceResult GetBDJKDetailList([DataSourceRequest] DataSourceRequest request, int Period, string NoReg)
        {
            var getDataPlanningDetailSummary = annualPlanningService.GetAnnualBDJKPlanningDetailSummary(Period, NoReg)
                .OrderBy(ob => ob.PlanDate).ToList();

            return getDataPlanningDetailSummary.ToDataSourceResult(request);
        }

        [HttpPost("get-total-leave-planning")]
        public int GetTotalLeavePlanning()
        {
            int Period = Convert.ToInt32(this.Request.Form["Period"]);
            string NoReg = this.Request.Form["NoReg"].ToString();
            var totalPlanningDetail = annualPlanningService.GetAnnualLeavePlanningDetail(Period, NoReg).Sum(x => x.Days);

            return totalPlanningDetail;
        }

        [HttpPost("get-hierarchies")]
        public List<AnnualPlanningHierarchiesStoredEntity> GetHierarchies()
        {
            string NoReg = Request.Form["NoReg"];
            return annualPlanningService.GetHierarchies(NoReg).Where(wh => wh.OrderRank==1).ToList();
        }

        //[HttpPost("get-structures")]
        //public async TreeDataSourceResult GetStructures([DataSourceRequest] DataSourceRequest request)
        //{
        //    string NoReg = Request.Form["NoReg"];
        //    var structures = annualPlanningService.GetHierarchies(NoReg).Where(wh => wh.OrderRank == 1);

        //    return structures.Select(x => new { x., x.ParentOrgCode, x.ObjectText, x.ObjectDescription,x.rel }).ToTreeDataSourceResult(request, e => e.OrgCode, e => e.ParentOrgCode, e => e);
        //}

        [HttpPost("get-subordinate-annual-planning")]
        public async Task<DataSourceResult> GetSubordinateAnnualPlanning(
            [DataSourceRequest] DataSourceRequest request,
            [FromForm] int? yearPeriod = null,
            [FromForm] string formKey = null)
        {
            string noReg = User.Claims.ElementAtOrDefault(0).Value;
            string postCode = User.Claims.ElementAtOrDefault(4).Value;

            return await annualPlanningService.GetSubordinateAnnualPlanningDashboard(noReg, yearPeriod, formKey)
            //return await annualPlanningService.GetSubordinateAnnualPlanningDashboard(noReg,postCode, yearPeriod, formKey)
                .ToDataSourceResultAsync(request);
        }

        // Temporary action for Annual Planning Dashboard Grid
        [HttpPost("get-personal-annual-planning")]
        public async Task<DataSourceResult> GetPersonalAnnualPlanning(
            [DataSourceRequest] DataSourceRequest request,
            [FromForm] int? yearPeriod = null,
            [FromForm] string formKey = null)
        {
            var test = User.Claims;
            string noReg = User.Claims.ElementAtOrDefault(0).Value;
            string postCode = User.Claims.ElementAtOrDefault(4).Value;
            return await annualPlanningService.GetPersonalAnnualPlanningDashboard(noReg, yearPeriod, formKey)
                .ToDataSourceResultAsync(request);
        }


        [HttpPost("get-year-period")]
        public Task<DataSourceResult> GetYearPeriod()
        {
            string noReg = User.Claims.ElementAtOrDefault(0).Value;
            string postCode = User.Claims.ElementAtOrDefault(4).Value;
            return annualPlanningService.GetYearPeriod(noReg, postCode)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-abnormalities")]
        public Task<DataSourceResult> GetAbnormalities()
        {
            string noReg = User.Claims.ElementAtOrDefault(0).Value;
            DateTime keyDate = DateTime.Today.AddDays(-1);
            return annualPlanningService.GetAbnormalities(noReg,keyDate)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-form-type")]
        public Task<DataSourceResult> GetFormTypes([FromForm] Boolean isChief)
        {

            IEnumerable<Form> ds = formService.Gets().Where(f => f.ModuleCode == "timemanagement" && f.FormKey.StartsWith("annual") && !f.FormKey.Contains("wfh"));

            if (!isChief)
            {
                ds = ds.Where(f => f.TitleFormat.Contains("leave"));
            }

            return ds.ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-division")]
        public List<DropDownTreeItemModel> GetDivision([FromForm] List<string> DirectorateList)
        {
            var divisions = ServiceProxy.GetService<DailyReconciliationService>().GetDivisionsFromMultiDirectorate(DirectorateList);

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
            var departments = ServiceProxy.GetService<DailyReconciliationService>().GetDepartmentsFromMultiDivision(DivisionList);

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
            var sections = ServiceProxy.GetService<DailyReconciliationService>().GetSectionsFromMultiDepartment(DepartmentList);

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
            var lines = ServiceProxy.GetService<DailyReconciliationService>().GetLinesFromMultiSection(SectionList);

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
            var groups = ServiceProxy.GetService<DailyReconciliationService>().GetGroupsFromMultiLine(LineList);

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

        [HttpGet("get-profpic-avatar")]
        public string GetProfpicAvatar(string NoReg)
        {
            var noregs = User.GetClaim("NoReg");
            return ConfigService.ResolveAvatar(noregs);
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime dateFrom, DateTime dateTo, string employeeName, string listDirectorate, string listDivision, string listDepartment, string listSection, string listLine, string listGroup, string className, string noReg)
        {
            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;
            var getData = annualPlanningService.GetAnnualPlanningReport(dateFrom, dateTo, NoReg, PostCode).ToList();

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

            if (!string.IsNullOrEmpty(noReg))
            {
                getData = getData.Where(c => c.NoReg.Contains(noReg)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class.Contains(className)).ToList();
            }

            var fileName = string.Format("Annual-Planning-Report-{0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", dateFrom, dateTo);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 3;
                    var sheet = package.Workbook.Worksheets.Add("Output");

                    var cols = new[] { "No Reg.", "Name", "Job Name", "Item", "JanuariPlan", "JanuariAct", "FebruariPlan", "FebruariAct", "MaretPlan", "MaretAct", "AprilPlan", "AprilAct", "MeiPlan", "MeiAct", "JuniPlan", "JuniAct", "JuliPlan", "JuliAct", "AgustusPlan", "AgustusAct", "SeptemberPlan", "SeptemberAct", "OktoberPlan", "OktoberAct", "NovemberPlan", "NovemberAct", "DesemberPlan", "DesemberAct", "TotalPlan", "TotalAct" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        if (cols[i - 1].Contains("Plan"))
                        {
                            sheet.Cells[1, i].Value = cols[i - 1].Substring(0, cols[i - 1].Length-4);
                            sheet.Cells[1, i, 1, i + 1].Merge = true;
                            sheet.Cells[2, i].Value = "Plan";
                            sheet.Cells[2, i + 1].Value = "Actual";

                            sheet.Cells[1, i, 1, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[1, i, 1, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[1, i, 1, i + 1].Style.Font.Bold = true;

                            sheet.Cells[2, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[2, i].Style.Font.Bold = true;

                            sheet.Cells[2, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[2, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[2, i + 1].Style.Font.Bold = true;
                            i++;
                        }
                        else
                        {
                            sheet.Cells[1, i].Value = cols[i - 1];
                            sheet.Cells[1, i].Style.Font.Bold = true;
                            sheet.Cells[1, i].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            sheet.Cells[1, i, 2, i].Merge = true;
                            sheet.Cells[1, i, 2, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                    }

                    var temporaryName = "";
                    var temporaryRowIndex = rowIndex;

                    foreach (var item in getData)
                    {
                        if(temporaryName != item.Name)
                        {
                            temporaryName = item.Name;

                            if (rowIndex != 3)
                            {
                                sheet.Cells[temporaryRowIndex, 1, rowIndex - 1, 1].Merge = true;
                                sheet.Cells[temporaryRowIndex, 2, rowIndex - 1, 2].Merge = true;
                                sheet.Cells[temporaryRowIndex, 3, rowIndex - 1, 3].Merge = true;
                                sheet.Cells[temporaryRowIndex, 1, rowIndex - 1, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }

                            temporaryRowIndex = rowIndex;
                        }

                        sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
                        sheet.Cells[rowIndex, 2].Value = item.Name;
                        sheet.Cells[rowIndex, 3].Value = item.JobName;
                        sheet.Cells[rowIndex, 4].Value = item.Title;
                        sheet.Cells[rowIndex, 5].Value = item.JanPlan;
                        sheet.Cells[rowIndex, 6].Value = item.JanAct;
                        sheet.Cells[rowIndex, 7].Value = item.FebPlan;
                        sheet.Cells[rowIndex, 8].Value = item.FebAct;
                        sheet.Cells[rowIndex, 9].Value = item.MarPlan;
                        sheet.Cells[rowIndex, 10].Value = item.MarAct;
                        sheet.Cells[rowIndex, 11].Value = item.AprPlan;
                        sheet.Cells[rowIndex, 12].Value = item.AprAct;
                        sheet.Cells[rowIndex, 13].Value = item.MayPlan;
                        sheet.Cells[rowIndex, 14].Value = item.MayAct;
                        sheet.Cells[rowIndex, 15].Value = item.JunPlan;
                        sheet.Cells[rowIndex, 16].Value = item.JunAct;
                        sheet.Cells[rowIndex, 17].Value = item.JulPlan;
                        sheet.Cells[rowIndex, 18].Value = item.JulAct;
                        sheet.Cells[rowIndex, 19].Value = item.AugPlan;
                        sheet.Cells[rowIndex, 20].Value = item.AugAct;
                        sheet.Cells[rowIndex, 21].Value = item.SepPlan;
                        sheet.Cells[rowIndex, 22].Value = item.SepAct;
                        sheet.Cells[rowIndex, 23].Value = item.OctPlan;
                        sheet.Cells[rowIndex, 24].Value = item.OctAct;
                        sheet.Cells[rowIndex, 25].Value = item.NovPlan;
                        sheet.Cells[rowIndex, 26].Value = item.NovAct;
                        sheet.Cells[rowIndex, 27].Value = item.DecPlan;
                        sheet.Cells[rowIndex, 28].Value = item.DecAct;
                        sheet.Cells[rowIndex, 29].Value = item.TotPlan;
                        sheet.Cells[rowIndex, 30].Value = item.TotAct;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                    
                        rowIndex++;
                    }

                    if (rowIndex - 1 > temporaryRowIndex)
                    {
                        sheet.Cells[temporaryRowIndex, 1, rowIndex - 1, 1].Merge = true;
                        sheet.Cells[temporaryRowIndex, 2, rowIndex - 1, 2].Merge = true;
                        sheet.Cells[temporaryRowIndex, 3, rowIndex - 1, 3].Merge = true;
                        sheet.Cells[temporaryRowIndex, 1, rowIndex - 1, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet("download-leave-planning-detail")]
        public IActionResult DownloadLeavePlanningDetail(int Period, string NoReg)
        {
            var getData = annualPlanningService.GetAnnualLeavePlanningDetailSummary(Period, NoReg).OrderBy(ob => ob.PlanDate).ToList();


            var fileName = string.Format("Annual-Leave-Planning-Detail-{0}-{1}.xlsx", NoReg, Period);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    
                    var sheet = package.Workbook.Worksheets.Add("Sheet1");

                    //sheet.Cells[1, 1].Value = NoReg;
                    //sheet.Cells[1, 2].Value = "Period "+Period;

                    var cols = new[] { "Plan Date", "Plan", "Actual", "Remark" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    int rowIndex = 2;
                    foreach (var item in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.PlanDate;
                        sheet.Cells[rowIndex, 2].Value = item.Plans;
                        sheet.Cells[rowIndex, 3].Value = item.Actual;
                        sheet.Cells[rowIndex, 4].Value = item.Remark;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            if (i == 1)
                            {
                                sheet.Cells[rowIndex, i].Style.Numberformat.Format = "dd/MM/yyyy";
                            }
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

        [HttpGet("download-wfh-planning-detail")]
        public IActionResult DownloadWFHPlanningDetail(int Period, string NoReg, string strDateFrom, string strDateTo)
        {
            var getData = annualPlanningService.GetAnnualWFHPlanningDetailSummary(Period, NoReg).OrderBy(ob => ob.PlanDate).ToList();

            //string strDateFrom = this.Request.Form["DateFrom"].ToString();
            //string strDateTo = this.Request.Form["DateTo"].ToString();

            DateTime dateFrom = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-01");
            DateTime dateTo = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            if (strDateFrom != "")
            {
                dateFrom = Convert.ToDateTime(strDateFrom);
            }

            if (strDateTo != "")
            {
                dateTo = Convert.ToDateTime(strDateTo);
            }

            getData = getData.Where(c => c.PlanDate >= dateFrom && c.PlanDate <= dateTo).ToList();

            var fileName = string.Format("Annual-WFH-Planning-Detail-{0}-{1:ddMMyyyy}-{2:ddMMyyyy}.xlsx",NoReg, dateFrom, dateTo);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    
                    var sheet = package.Workbook.Worksheets.Add("Sheet1");

                    var cols = new[] { "Plan Date", "Plan", "Actual", "Remark"};

                    for (var i = 1; i <= cols.Length; i++)
                    {
                            sheet.Cells[1, i].Value = cols[i - 1];
                            sheet.Cells[1, i].Style.Font.Bold = true;
                            sheet.Cells[1, i].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    int rowIndex = 2;
                    foreach (var item in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.PlanDate;
                        sheet.Cells[rowIndex, 2].Value = item.Plans;
                        sheet.Cells[rowIndex, 3].Value = item.Actual;
                        sheet.Cells[rowIndex, 4].Value = item.Remark;
                        
                        for (var i = 1; i <= cols.Length; i++)
                        {
                            if (i == 1)
                            {
                                sheet.Cells[rowIndex, i].Style.Numberformat.Format = "dd/MM/yyyy";
                            }
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

        [HttpGet("download-bdjk-planning-detail")]
        public IActionResult DownloadBDJKPlanningDetail(int Period, string NoReg, string strDateFrom, string strDateTo)
        {
            var getData = annualPlanningService.GetAnnualBDJKPlanningDetailSummary(Period, NoReg).OrderBy(ob => ob.PlanDate).ToList();

            DateTime dateFrom = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-01");
            DateTime dateTo = Convert.ToDateTime(DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            if (strDateFrom != "")
            {
                dateFrom = Convert.ToDateTime(strDateFrom);
            }

            if (strDateTo != "")
            {
                dateTo = Convert.ToDateTime(strDateTo);
            }

            getData = getData.Where(c => c.PlanDate >= dateFrom && c.PlanDate <= dateTo).ToList();

            var fileName = string.Format("Annual-BDJK-Planning-Detail-{0}-{1:ddMMyyyy}-{2:ddMMyyyy}.xlsx", NoReg, dateFrom, dateTo);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {

                    var sheet = package.Workbook.Worksheets.Add("Sheet1");

                    var cols = new[] { "Plan Date", "Plan", "Actual", "Remark" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    int rowIndex = 2;
                    foreach (var item in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.PlanDate;
                        sheet.Cells[rowIndex, 2].Value = item.Plans;
                        sheet.Cells[rowIndex, 3].Value = item.Actual;
                        sheet.Cells[rowIndex, 4].Value = item.Remark;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            if (i == 1)
                            {
                                sheet.Cells[rowIndex, i].Style.Numberformat.Format = "dd/MM/yyyy";
                            }
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
    //[Permission(PermissionKey.ReportAnnualPlanning)]
    public class AnnualPlanningController : MvcControllerBase
    {
        protected AnnualPlanningService annualPlanningService => ServiceProxy.GetService<AnnualPlanningService>();
 
        protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }
        public IActionResult Index()
        {
            string noReg = ServiceProxy.UserClaim.NoReg;
            string postCode = ServiceProxy.UserClaim.PostCode;
            ViewBag.NoReg = noReg;
            ViewBag.treeData = GetHierarchiest(noReg, postCode);
            ViewBag.directorateData = GetDirectorateTree();
            return View();
        }

        public IActionResult ViewForm(int Period, string NoReg,string Name, string PostName, int SequenceNo)
        {
            //var data = annualPlanningService.GetAnnualPlanningSummary(Period,NoReg).Where(wh => wh.SequenceNo==SequenceNo);
            //var data = new
            //{
            //    Period= Period,
            //    NoReg = NoReg
            //};
            var profileData = MdmService.GetActualOrganizationStructure(NoReg);
            ViewBag.Period = Period;
            ViewBag.NoReg = NoReg;
            ViewBag.Name = Name;
            ViewBag.PostName = profileData.PostName;
            string FormView = "";
            if (SequenceNo == 2)
            {
                FormView = "_LeaveFormView";
            }else if (SequenceNo == 4)
            {
                FormView = "_WFHFormView";
            }
            else if (SequenceNo == 6)
            {
                ViewBag.ListBDJKCategory = ConfigService.GetGeneralCategories("BdjkCode").ToList();
                FormView = "_BDJKFormView";
            }
            return View("~/Areas/TimeManagement/Views/AnnualPlanning/Form/"+FormView+".cshtml");
        }

        public List<TreeViewItemModel> GetHierarchiest(string noreg, string postCode)
        {

            // Set key date.
            var keyDate = DateTime.UtcNow;

            // Get list of one level hierarchy by noreg, position code, and key date.
            var output = annualPlanningService.GetEmployeeHierarchies(noreg,postCode,keyDate);

            // Get list of superiors.
            var hierarchiest = output.Where(x => x.RelationType == "Hierarchy");

            // Get list of subordinates.
            var subordinates = output.Where(x => x.RelationType == "Subordinate");

            // Get current user session organization.
            var selfObject = output.First();

            // Create new TreeViewItemModel for current user session organization.
            var self = new TreeViewItemModel
            {
                Id = selfObject.ActualNoReg + "#" + selfObject.ActualPostCode,
                // Get and set text information.
                Text = selfObject.ActualStaffing == 0 ? string.Format(@"<span style=""cursor: help;"" class=""font-red-sunglo"" data-toggle=""tooltip"" data-html=""true"" data-container=""body"" title=""{1}"">{0} (*)</span>", selfObject.ActualName, selfObject.ActualPostName) : selfObject.ActualName,
                // Get and set IPP document url.
                Url = Url.Content("~/timemanagement/annualplanning/index#APF"),
                // Set expanded by default.
                Expanded = true
            };

            // Create new TreeViewItemModel list object.
            var inlineDefault = new List<TreeViewItemModel>();

            inlineDefault.Add(self);

            // Get last item of the list.
            var root = inlineDefault.Last();

            // If current user session organization data not exist in the list then add it into the sub-list.
            if (root != self)
            {
                // Add current user session organization into the sub-list.
                root.Items.Add(self);
            }

            // Enumerate through subordinates data.
            foreach (var subordinate in subordinates)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                self.Items.Add(new TreeViewItemModel
                {
                    Id = subordinate.TargetNoReg + "#" + subordinate.TargetPostCode,
                    // Get and set text information.
                    Text = subordinate.TargetStaffing == 0 ? string.Format(@"<span style=""cursor: help;"" class=""font-red-sunglo"" data-toggle=""tooltip"" data-html=""true"" data-container=""body"" title=""{1}"">{0} (*)</span>", subordinate.TargetName, subordinate.TargetPostName) : subordinate.TargetName,
                    // Get and set IPP document url.
                    Url = Url.Content("~/timemanagement/annualplanning/index#APF"),
                    // Set expanded by default.
                    Expanded = true
                });
                //GetChildHierarchiest(subordinate.TargetNoReg, subordinate.TargetPostCode);


            }

            
            // Return partial view with given view model.
            return inlineDefault;
        }

        public List<TreeViewItemModel> GetChildHierarchiest(string noreg, string postCode)
        {

            // Set key date.
            var keyDate = DateTime.UtcNow;

            // Get list of one level hierarchy by noreg, position code, and key date.
            var output = annualPlanningService.GetEmployeeHierarchies(noreg, postCode, keyDate);

            // Get list of superiors.
            var hierarchiest = output.Where(x => x.RelationType == "Hierarchy");

            // Get list of subordinates.
            var subordinates = output.Where(x => x.RelationType == "Subordinate");

            // Get current user session organization.
            var selfObject = output.First();

            // Create new TreeViewItemModel for current user session organization.
            var self = new TreeViewItemModel
            {
                Id = selfObject.ActualNoReg,
                // Get and set text information.
                Text = selfObject.ActualStaffing == 0 ? string.Format(@"<span style=""cursor: help;"" class=""font-red-sunglo"" data-toggle=""tooltip"" data-html=""true"" data-container=""body"" title=""{1}"">{0} (*)</span>", selfObject.ActualName, selfObject.ActualPostName) : selfObject.ActualName,
                // Get and set IPP document url.
                Url = Url.Content("~/timemanagement/annualplanning/index#APF"),
                // Set expanded by default.
                Expanded = true
            };

            // Create new TreeViewItemModel list object.
            var inlineDefault = new List<TreeViewItemModel>();

            inlineDefault.Add(self);

            // Get last item of the list.
            var root = inlineDefault.Last();

            // If current user session organization data not exist in the list then add it into the sub-list.
            if (root != self)
            {
                // Add current user session organization into the sub-list.
                root.Items.Add(self);
            }

            // Enumerate through subordinates data.
            foreach (var subordinate in subordinates)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                self.Items.Add(new TreeViewItemModel
                {
                    Id = subordinate.TargetNoReg,
                    // Get and set text information.
                    Text = subordinate.TargetStaffing == 0 ? string.Format(@"<span style=""cursor: help;"" class=""font-red-sunglo"" data-toggle=""tooltip"" data-html=""true"" data-container=""body"" title=""{1}"">{0} (*)</span>", subordinate.TargetName, subordinate.TargetPostName) : subordinate.TargetName,
                    // Get and set IPP document url.
                    Url = Url.Content("~/timemanagement/annualplanning/index#APF"),
                    // Set expanded by default.
                    Expanded = true
                });
                GetChildHierarchiest(subordinate.TargetNoReg, subordinate.TargetPostCode);
            }

            // Return partial view with given view model.
            return inlineDefault;
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
