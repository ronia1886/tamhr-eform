using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agit.Common.Extensions;
using FluentValidation.Validators;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.TimeManagement;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API
    [Route("api/weekly-wfh-report")]
    public class WeeklyWFHReportAPIController : ApiControllerBase
    {
        public class EmployeeSummary
        {
            public string NoReg, Name, PostName, JobName, Division, Department, Section, Line, Group, Class;
            public decimal PlanActualPercent, PlanMinWFO, ActualMinWFO, PlanCountWorkDay, ActualCountWorkDay;
        }

        public class EmployeeTemporarySummary
        {
            public string NoReg, WorkPlace;
            public DateTime Date;
        }

        protected WeeklyWFHPlanningService weeklyWFHPlanningService => ServiceProxy.GetService<WeeklyWFHPlanningService>();
        protected WeeklyWFHReportService weeklyWFHReportService => ServiceProxy.GetService<WeeklyWFHReportService>();
        protected UserService userService => ServiceProxy.GetService<UserService>();
        protected MdmService mdmService => ServiceProxy.GetService<MdmService>();

        private bool isPlanExist { get; set; }

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

            var data = weeklyWFHPlanningService.GetWeeklyWFHPlannings()
                .ToList();
            
            return data.ToDataSourceResult(request);
        }

        /// <summary>
        /// Get weekly wfh report location summary
        /// </summary>
        /// <remarks>
        /// Get weekly wfh report location summary
        /// </remarks>
        [HttpPost("schedule-summary")]
        public IEnumerable<dynamic> GetSchedulePlanSummary([DataSourceRequest] DataSourceRequest request)
        {
            string keyDate = this.Request.Form["param[keyDate]"].ToString();
            string listDirectorate = this.Request.Form["param[directorateList][]"].ToString();
            string listDivision = this.Request.Form["param[divisionList][]"].ToString();
            string listDepartment = this.Request.Form["param[departmentList][]"].ToString();
            string listSection = this.Request.Form["param[sectionList][]"].ToString();
            string listLine = this.Request.Form["param[lineList][]"].ToString();
            string listGroup = this.Request.Form["param[groupList][]"].ToString();
            string className = this.Request.Form["param[class]"].ToString();
            string actualLocation = this.Request.Form["param[actualLocation]"].ToString();
            string planLocation = this.Request.Form["param[planLocation]"].ToString();
            var TempNoReg = this.Request.Form["param[selectedNoReg]"].ToString();
            var TempPostCode = this.Request.Form["param[selectedPostCode]"].ToString();
            bool firstLoad = Boolean.Parse(this.Request.Form["param[firstLoad]"].ToString());

            DateTime convDate;
            try
            {
                convDate = DateTime.Parse(keyDate);
            }
            catch
            {
                convDate = DateTime.Parse(keyDate.Split('/')[2] + '-' + keyDate.Split('/')[1] + '-' + keyDate.Split('/')[0]);
            }

            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            IEnumerable<WeeklyWFHDownloadStoredEntity> getData = null;
            //WFHGeneratePlanningDateWeekly wpd = weeklyWFHPlanningService.GetWeeklyWFHPlanningByDate(convDate, 14, true);
            //WFHGeneratePlanningDateMonthly wpd = weeklyWFHPlanningService.GetMonthlyWFHPlanningByDate(convDate);

            // Get the first day of the month
            DateTime firstDayOfMonth = new DateTime(convDate.Year, convDate.Month, 1);

            // Get the first day of the next month
            DateTime firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            // Get the last day of the month
            DateTime lastDayOfMonth = firstDayOfNextMonth.AddDays(-1);

            if (firstLoad)
            {
                getData = weeklyWFHReportService.GetWeeklyWFHDownloadReport(firstDayOfMonth, lastDayOfMonth, noreg, postCode);
            }
            else
            {
                getData = weeklyWFHReportService.GetWeeklyWFHDownloadReport(firstDayOfMonth, lastDayOfMonth, null, null);
            }

            

            if (!string.IsNullOrEmpty(listDirectorate) && listDirectorate.Split(",").Length > 0)
            {
                var newText = stringCombine(listDirectorate);
                getData = getData.Where(c => newText.Contains(c.Directorate)).ToList();
            }

            if (!string.IsNullOrEmpty(listDivision) && listDivision.Split(",").Length > 0)
            {
                var newText = stringCombine(listDivision);
                getData = getData.Where(c => newText.Contains(c.Division)).ToList();
            }

            if (!string.IsNullOrEmpty(listDepartment) && listDepartment.Split(",").Length > 0)
            {
                var newText = stringCombine(listDepartment);
                getData = getData.Where(c => newText.Contains(c.Department)).ToList();
            }

            if (!string.IsNullOrEmpty(listSection) && listSection.Split(",").Length > 0)
            {
                var newText = stringCombine(listSection);
                getData = getData.Where(c => newText.Contains(c.Section)).ToList();
            }

            if (!string.IsNullOrEmpty(listLine) && listLine.Split(",").Length > 0)
            {
                var newText = stringCombine(listLine);
                getData = getData.Where(c => newText.Contains(c.Line)).ToList();
            }

            if (!string.IsNullOrEmpty(listGroup) && listGroup.Split(",").Length > 0)
            {
                var newText = stringCombine(listGroup);
                getData = getData.Where(c => newText.Contains(c.Group)).ToList();
            }


            if (!string.IsNullOrEmpty(className))
            {
                getData = getData.Where(c => c.Class.Contains(className)).ToList();
            }

            if (!string.IsNullOrEmpty(TempNoReg))
            {
                getData = getData.Where(c => c.NoReg.Contains(TempNoReg)).ToList();
            }

            if (!string.IsNullOrEmpty(TempPostCode))
            {
                getData = getData.Where(c => c.PostCode.Contains(TempPostCode)).ToList();
            }


            var getDataPlan = getData.Where(a => a.Type == "Plan").ToList();
            var getDataActual = getData.Where(a => a.Type == "Actual").ToList();

            if (!string.IsNullOrEmpty(planLocation))
            {
                var isExist = getDataPlan.Any(c=>c.WorkPlace == planLocation && c.Date == convDate);
                if (!isExist)
                {
                    getDataPlan = new List<WeeklyWFHDownloadStoredEntity>();
                    getDataActual = new List<WeeklyWFHDownloadStoredEntity>();
                }
            }

            if (!string.IsNullOrEmpty(actualLocation))
            {
                var isExist = getDataActual.Any(c => c.WorkPlace == actualLocation && c.Date == convDate);
               
                if (!isExist)
                {
                    
                    getDataPlan = new List<WeeklyWFHDownloadStoredEntity>();
                    getDataActual = new List<WeeklyWFHDownloadStoredEntity>();
                }
            }


            var listgeneralCategory = weeklyWFHReportService.GetGeneralCategories("HybridSchedule").ToList();

            var resultActual = (from a in listgeneralCategory
                         join b in getDataActual on a.Code equals b.WorkPlace into c
                         from d in c.DefaultIfEmpty()
                         group d by a.Code into grouped
                         select new { Type = "Actual",Code = grouped.Key, Name = grouped.Key, Total = grouped.Count(t => t != null) }).ToList();

            var resultPlan = (from a in listgeneralCategory.Where(x=>x.Description.Contains("True")).ToList()
                             join b in getDataPlan on a.Code equals b.WorkPlace into c
                             from d in c.DefaultIfEmpty()
                             group d by a.Code into grouped
                             select new { Type = "Plan",Code = grouped.Key, Name = grouped.Key, Total = grouped.Count(t => t != null) });

            var resultActualLocation = (from a in listgeneralCategory
                                        join b in getDataActual.Where(data => data.Date >= firstDayOfMonth && data.Date <= lastDayOfMonth)
                                        on a.Code equals b.WorkPlace into c
                                        from d in c.DefaultIfEmpty()
                                        group d by a.Code into grouped
                                        select new
                                        {
                                            Type = "ActualLocation",
                                            Code = grouped.Key,
                                            Name = grouped.Key,
                                            Total = grouped.Count(t => t != null)
                                        }).ToList();

            var result = new List<dynamic>();
            result.AddRange(resultActual);
            result.AddRange(resultPlan);
            result.AddRange(resultActualLocation);


            return result;
        }


        [HttpPost("charts-summary")]
        public IActionResult GetChartsSummary([FromBody] WeeklyWFHChartSummary param)
        {
            if (param == null || string.IsNullOrEmpty(param.KeyDate))
            {
                return BadRequest("KeyDate is required.");
            }

            // Initialize convDate and try to parse KeyDate
            DateTime convDate = DateTime.MinValue;
            bool isParsed = false;
            string[] cultures = { "en-US", "id-ID" };

            foreach (var culture in cultures)
            {
                if (DateTime.TryParseExact(param.KeyDate, "dd MMMM yyyy", new CultureInfo(culture), DateTimeStyles.None, out convDate))
                {
                    isParsed = true;
                    break;
                }
            }

            if (!isParsed)
            {
                return BadRequest("Invalid date format for KeyDate.");
            }

            // Declare getData variable
            IEnumerable<WeeklyWFHChartSummary> getData = Enumerable.Empty<WeeklyWFHChartSummary>();

            var Noreg = ServiceProxy.UserClaim.NoReg;
            // Prepare parameters as chief
            string employeeName = param.employeeName;
            string directorate = param.Directorate;
            string division = param.Division;
            string department = param.Department;
            string section = param.Section;
            string line = param.Line;
            string group = param.Group;
            string className = param.Class;
            string planWorkPlace = param.planWorkPlace;
            string actualWorkPlace = param.actualWorkPlace;
            string noreg = Noreg;
            string postCode = ServiceProxy.UserClaim.PostCode;
            string firstload = param.FirstLoad;
            string noregSuperior = "";
            string postCodeSuperior = "";
            // Fetch data based on user role
            if (ServiceProxy.UserClaim.Chief)
            {
                // Call service for Chief
                //getData = weeklyWFHReportService.GetWeeklyWFHChartsReportSuperior(
                //    param.KeyDate,
                //    employeeName,
                //    directorate,
                //    division,
                //    department,
                //    section,
                //    line,
                //    group,
                //    className,
                //    planWorkPlace,
                //    actualWorkPlace,
                //    noreg,
                //    firstload);
                //getData = weeklyWFHReportService.GetWeeklyWFHChartsReportSuperior(
                //    param.KeyDate,
                //    employeeName,
                //    directorate,
                //    division,
                //    department,
                //    section,
                //    line,
                //    group,
                //    className,
                //    planWorkPlace,
                //    actualWorkPlace,
                //    noreg,
                //    firstload);

                noregSuperior = ServiceProxy.UserClaim.NoReg;
                postCodeSuperior = ServiceProxy.UserClaim.PostCode;
            }
            else
            {
                
                // Prepare parameters as employee
                employeeName = param.employeeName;
                directorate = param.Directorate;
                division = param.Division;
                department = param.Department;
                section = param.Section;
                line = param.Line;
                group = param.Group;
                className = param.Class;
                planWorkPlace = param.planWorkPlace;
                actualWorkPlace = param.actualWorkPlace;
                //noreg = param.NoReg;
                firstload = param.FirstLoad;

                //default filter
                if (directorate.IsNullOrEmpty()
                 && division.IsNullOrEmpty()
                 && department.IsNullOrEmpty()
                 && section.IsNullOrEmpty()
                 && line.IsNullOrEmpty()
                 && group.IsNullOrEmpty()
                )
                {
                    //get data superior berdasarkan noreg login
                    var dataSuperior = weeklyWFHPlanningService.GetActualReportingStructure(User.GetClaim("NoReg"), User.GetClaim("PostCode"));
                    if (dataSuperior != null)
                    {
                        noregSuperior = dataSuperior.ParentNoReg;
                        postCodeSuperior = dataSuperior.ParentPostCode;
                    }

                }

                // Call service for non-Chief
                //getData = weeklyWFHReportService.GetWeeklyWFHChartsReport(
                //    param.KeyDate,
                //    employeeName,
                //    directorate,
                //    division,
                //    department,
                //    section,
                //    line,
                //    group,
                //    className,
                //    planWorkPlace,
                //    actualWorkPlace,
                //    noreg,
                //    firstload);
            }

            //// Filter data (if necessary)
            //// Filter data (if necessary)
            //var getDataPlan = getData.Where(a => a.Type == "Plan" && a.Date >= convDate && a.Date < convDate.AddMonths(1)).ToList();
            //var getDataActual = getData.Where(a => a.Type == "Actual" && a.Date >= convDate && a.Date < convDate.AddMonths(1)).ToList();

            //// Debug: Log data counts
            //Console.WriteLine($"Plan Data Count: {getDataPlan.Count}");
            //Console.WriteLine($"Actual Data Count: {getDataActual.Count}");

            //if (!string.IsNullOrEmpty(param.planWorkPlace))
            //{
            //    var isExist = getDataPlan.Any(c => c.WorkPlace == param.planWorkPlace);
            //    if (!isExist)
            //    {
            //        getDataPlan = new List<WeeklyWFHChartSummary>();
            //        getDataActual = new List<WeeklyWFHChartSummary>();
            //    }
            //}

            //if (!string.IsNullOrEmpty(param.actualWorkPlace))
            //{
            //    var isExist = getDataActual.Any(c => c.WorkPlace == param.actualWorkPlace);
            //    if (!isExist)
            //    {
            //        getDataPlan = new List<WeeklyWFHChartSummary>();
            //        getDataActual = new List<WeeklyWFHChartSummary>();
            //    }
            //}

            //var listGeneralCategory = weeklyWFHReportService.GetGeneralCategories("HybridSchedule").ToList();

            //// Log categories
            //foreach (var category in listGeneralCategory)
            //{
            //    Console.WriteLine($"Category: {category.Code}, Description: {category.Description}");
            //}
            //var excludedCodes = new List<string> { "WFO Created By System - True (tambahan true ini untuk data chart apa aja yang akan muncul untuk plan summary)" };

            //var resultPlan = (from a in listGeneralCategory.Where(x => x.Description.Contains("True")).ToList()
            //                  join b in getDataPlan on a.Code equals b.WorkPlace into c
            //                  from d in c.DefaultIfEmpty()
            //                  group d by a.Code into grouped
            //                  select new
            //                  {
            //                      Type = "Plan",
            //                      Code = grouped.Key,
            //                      Name = grouped.Key,
            //                      Total = grouped.Count(t => t != null)
            //                  }).ToList();

            //var resultActual = (from a in listGeneralCategory.Where(a => !excludedCodes.Contains(a.Description))
            //                    join b in getDataActual on a.Code equals b.WorkPlace into c
            //                    from d in c.DefaultIfEmpty()
            //                    group d by a.Code into grouped
            //                    select new
            //                    {
            //                        Type = "Actual",
            //                        Code = grouped.Key,
            //                        Name = grouped.Key,
            //                        Total = grouped.Count(t => t != null)
            //                    }).ToList();

            //var resultActualLocation = (from a in listGeneralCategory.Where(a => !excludedCodes.Contains(a.Description))
            //                            join b in getDataActual.Where(data => data.Date >= convDate && data.Date < convDate.AddMonths(1))
            //                            on a.Code equals b.WorkPlace into c
            //                            from d in c.DefaultIfEmpty()
            //                            group d by a.Code into grouped
            //                            select new
            //                            {
            //                                Type = "ActualLocation",
            //                                Code = grouped.Key,
            //                                Name = grouped.Key,
            //                                Total = grouped.Count(t => t != null)
            //                            }).ToList();

            //// Debug: Log result counts
            //Console.WriteLine($"Result Plan Count: {resultPlan.Count}");
            //Console.WriteLine($"Result Actual Count: {resultActual.Count}");
            //Console.WriteLine($"Result ActualLocation Count: {resultActualLocation.Count}");

            var getDataSimplify = weeklyWFHReportService.GetWeeklyWFHChartsReportSimplify(
                    param.KeyDate,
                    employeeName,
                    directorate,
                    division,
                    department,
                    section,
                    line,
                    group,
                    className,
                    planWorkPlace,
                    actualWorkPlace,
                    noreg,
                    postCode,
                    firstload,
                    noregSuperior,
                    postCodeSuperior);

            var result = new List<dynamic>();
            //result.AddRange(resultActual);
            //result.AddRange(resultPlan);
            //result.AddRange(resultActualLocation);
            var finalData = getDataSimplify.Select(obj => new { Type = obj.Category, Code = obj.Code, Name = obj.Code, Total = Convert.ToInt32(obj.Name) }).OrderBy(ob => ob.Type).ThenBy(ob => ob.Code).ToList();
            result.AddRange(finalData.Where(wh => wh.Type == "Actual").ToList());
            result.AddRange(finalData.Where(wh => wh.Type == "Plan").ToList());
            result.AddRange(finalData.Where(wh => wh.Type == "ActualLocation").ToList());

            return Ok(result); // Return the result as a JSON response

        }

        private string[] stringCombine(string text)
        {
            var splitString = text.Split(',');
            var sb = new StringBuilder();
            foreach (var item in splitString)
            {
                var data = item.Split('#')[1];
                sb.Append(data + ",");
            }
            var result = sb.ToString().Split(',');
            return result;
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
        public List<DropDownTreeItemModel> GetSection([FromForm] List<string> DepartmentList, [FromForm] List<string> DivisionList)
        {
            var sections = ServiceProxy.GetService<DailyReconciliationService>().GetSectionsFromMultiDepartmentAndDivision(DepartmentList,DivisionList);

            //var test = sections.Where(x=> DepartmentList.Contains(x.ParentOrgCode)).ToList();
            //var test2 = sections.Where(x => DivisionList.Contains(x.ParentOrgCode)).ToList();
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

        [HttpPost("get-class")]
        public async Task<DataSourceResult> GetClass()
        {
            return await weeklyWFHPlanningService.GetClass().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-wfh-planning-date/{keyDateStr}")]
        public WFHGeneratePlanningDateWeekly GetWFHPlanningDate(string keyDateStr)
        {
            WFHGeneratePlanningDateWeekly wFHGeneratePlanningDateWeekly = weeklyWFHPlanningService.GetWeeklyWFHPlanningByDate(DateTime.Parse(keyDateStr), 14, true);

            return wFHGeneratePlanningDateWeekly;
        }

        [HttpPost("get-reports")]
        public DataSourceResult GetReport([DataSourceRequest] DataSourceRequest request)
        {
            string keyDateStr = this.Request.Form["KeyDate"].ToString();

            bool firstLoad = Boolean.Parse(this.Request.Form["firstLoad"].ToString());

            string actualLocation = this.Request.Form["actualLocation"].ToString();

            string planLocation = this.Request.Form["planLocation"].ToString();

            string selectedDay = this.Request.Form["selectedDay"].ToString();

            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;

            var TempNoReg = this.Request.Form["selectedNoReg"].ToString();
            var TempPostCode = this.Request.Form["selectedPostCode"].ToString();

            WFHGeneratePlanningDateWeekly wpd = weeklyWFHPlanningService.GetWeeklyWFHPlanningByDate(DateTime.Parse(keyDateStr), 14, true);

            IEnumerable<WeeklyWFHStoredEntity> weeklyWFHStoredEntities;

            if (firstLoad)
            {
                weeklyWFHStoredEntities = weeklyWFHReportService.GetWeeklyWFHReport(wpd.StartDate, wpd.EndDate, NoReg, PostCode);
            } else if (!string.IsNullOrEmpty(TempNoReg) && !string.IsNullOrEmpty(TempPostCode))
            {
                weeklyWFHStoredEntities = weeklyWFHReportService.GetWeeklyWFHReport(wpd.StartDate, wpd.EndDate, TempNoReg, TempPostCode);
            }
            else
            {
                weeklyWFHStoredEntities = weeklyWFHReportService.GetWeeklyWFHReport(wpd.StartDate, wpd.EndDate);
            }

            var distinctNoReg = weeklyWFHStoredEntities.Select(x => x.NoReg).Distinct().ToList();

            List<string> filterNoReg = new List<string>();

            if((!string.IsNullOrEmpty(actualLocation) || !string.IsNullOrEmpty(planLocation)) && !string.IsNullOrEmpty(selectedDay))
            {
                foreach(string nr in distinctNoReg)
                {
                    var tempactual = weeklyWFHStoredEntities.Where(x => x.Type == "Actual" && (string.IsNullOrEmpty(actualLocation) ? true : (string)x.GetType().GetProperty(selectedDay).GetValue(x, null) == actualLocation) && x.NoReg == nr);
                    var tempplan = weeklyWFHStoredEntities.Where(x => x.Type == "Plan" && (string.IsNullOrEmpty(planLocation) ? true : (string)x.GetType().GetProperty(selectedDay).GetValue(x, null) == planLocation) && x.NoReg == nr);
                    
                    if(tempactual.ToList().Count == 0 || tempplan.ToList().Count == 0)
                    {
                        filterNoReg.Add(nr);
                    }
                }
            }



            var result = weeklyWFHStoredEntities.Where(x => !filterNoReg.Contains(x.NoReg)).ToDataSourceResult(request);

            //result.Total = distinctNoReg.Count();

            return result;
        }
        [HttpPost("events")]
        public async Task<IActionResult> GetEvents([FromBody] WeeklyWFHPlanningDetail model)
        {
            if (model == null)
            {
                return BadRequest("Model cannot be null.");
            }

            try
            {
                var noreg = ServiceProxy.UserClaim.NoReg;
                var postCode = ServiceProxy.UserClaim.PostCode;
                bool isChief = User.GetClaim("Chief") == "True";

                DateTime? startDate = null;

                if (!string.IsNullOrWhiteSpace(model.StartDate))
                {
                    var dateFormats = new[]
                    {
                "MMMM yyyy" // Default format
            };

                    var cultures = new[]
                    {
                new CultureInfo("id-ID"), // Bahasa Indonesia
                CultureInfo.InvariantCulture // English
            };

                    bool parsed = false;
                    DateTime parsedDate;

                    foreach (var culture in cultures)
                    {
                        foreach (var format in dateFormats)
                        {
                            if (DateTime.TryParseExact(
                                model.StartDate,
                                format,
                                culture,
                                DateTimeStyles.None,
                                out parsedDate))
                            {
                                startDate = parsedDate;
                                parsed = true;
                                break;
                            }
                        }
                        if (parsed) break;
                    }

                    if (!parsed)
                    {
                        return BadRequest("Invalid StartDate format. Expected format is 'MMMM yyyy' in English or Bahasa.");
                    }
                }

                string startDateString = startDate.HasValue ? startDate.Value.ToString("MMMM yyyy") : DateTime.Now.ToString("MMMM yyyy");

                //if (!startDate.HasValue &&
                //    string.IsNullOrWhiteSpace(model.EmployeeName) &&
                //    string.IsNullOrWhiteSpace(model.Directorate) &&
                //    string.IsNullOrWhiteSpace(model.Division) &&
                //    string.IsNullOrWhiteSpace(model.Department) &&
                //    string.IsNullOrWhiteSpace(model.Section) &&
                //    string.IsNullOrWhiteSpace(model.Line) &&
                //    string.IsNullOrWhiteSpace(model.Group) &&
                //    string.IsNullOrWhiteSpace(model.ClassName) &&
                //    string.IsNullOrWhiteSpace(model.PlanWorkPlace) &&
                //    string.IsNullOrWhiteSpace(model.ActualWorkPlace))
                //{
                //    var events = await weeklyWFHReportService.GetEventsAsync(noreg, isChief);
                //    if (events == null || events.Count == 0)
                //    {
                //        return NotFound("No events found for the current month and year.");
                //    }
                //    return Ok(events);
                //}
                //else
                //{
                //    List<WeeklyWFHPlanningDetail> events;

                //    if (isChief)
                //    {
                //        events = await weeklyWFHReportService.GetFiltersSuperiorAsync(
                //            noreg,
                //            startDateString,
                //            model.EmployeeName,
                //            model.Directorate,
                //            model.Division,
                //            model.Department,
                //            model.Section,
                //            model.Line,
                //            model.Group,
                //            model.ClassName,
                //            model.PlanWorkPlace,
                //            model.ActualWorkPlace
                //        );
                //    }
                //    else
                //    {
                //events = await weeklyWFHReportService.GetFiltersAsync(
                //    startDateString,
                //    model.EmployeeName,
                //    model.Directorate,
                //    model.Division,
                //    model.Department,
                //    model.Section,
                //    model.Line,
                //    model.Group,
                //    model.ClassName,
                //    model.PlanWorkPlace,
                //    model.ActualWorkPlace
                //);

                //}


                //    if (events == null || events.Count == 0)
                //    {
                //        return NotFound("No events found for the given criteria.");
                //    }

                //    return Ok(events);
                //}

                string noregSuperior = "";
                string postcodeSuperior = "";
                if (isChief)
                {
                    noregSuperior = User.GetClaim("NoReg");
                    postcodeSuperior = User.GetClaim("PostCode");
                }
                else
                {
                    //default filter
                    if(model.Directorate.IsNullOrEmpty()
                     && model.Division.IsNullOrEmpty()
                     && model.Department.IsNullOrEmpty()
                     && model.Section.IsNullOrEmpty()
                     && model.Line.IsNullOrEmpty()
                     && model.Group.IsNullOrEmpty()
                    )
                    {
                        //get data superior berdasarkan noreg login
                        var dataSuperior = weeklyWFHPlanningService.GetActualReportingStructure(User.GetClaim("NoReg"), User.GetClaim("PostCode"));
                        if(dataSuperior != null)
                        {
                            noregSuperior = dataSuperior.ParentNoReg;
                            postcodeSuperior = dataSuperior.ParentPostCode;
                        }
                        
                    }
                }
                var events = await weeklyWFHReportService.GetDataReport(
                    startDateString,
                    model.EmployeeName,
                    model.Directorate,
                    model.Division,
                    model.Department,
                    model.Section,
                    model.Line,
                    model.Group,
                    model.ClassName,
                    model.PlanWorkPlace,
                    model.ActualWorkPlace,
                    noregSuperior,
                    postcodeSuperior,
                    noreg,
                    postCode

                );
                return Ok(events);
            }
            catch (Exception ex)
            {
                // Use ILogger or a similar logging framework instead of Console.WriteLine
                Console.WriteLine($"Error retrieving events: {ex.Message}");
                return StatusCode(500, $"An error occurred while retrieving events: {ex.Message}");
            }
        }


        [HttpPost("events-superior")]
        public async Task<IActionResult> GetEventsSuperior([FromBody] WeeklyWFHPlanningDetail model)
        {
            try
            {
                // Retrieve NoReg from the service proxy
                var noreg = ServiceProxy.UserClaim.NoReg;

                // Default value for startDate
                DateTime? startDate = null;

                // Parse the StartDate if it's provided
                if (!string.IsNullOrWhiteSpace(model.StartDate))
                {
                    if (!DateTime.TryParseExact(model.StartDate, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        return BadRequest("Invalid StartDate format. Expected format is 'MMMM yyyy'.");
                    }
                    startDate = parsedDate;
                }

                // Convert startDate to a formatted string if it has a value
                string startDateString = startDate.HasValue ? startDate.Value.ToString("MMMM yyyy") : null;

                // Check if it's the initial load or a filtered request
                if (model == null ||
                    !startDate.HasValue &&
                    string.IsNullOrWhiteSpace(model.EmployeeName) &&
                    string.IsNullOrWhiteSpace(model.Directorate) &&
                    string.IsNullOrWhiteSpace(model.Division) &&
                    string.IsNullOrWhiteSpace(model.Department) &&
                    string.IsNullOrWhiteSpace(model.Section) &&
                    string.IsNullOrWhiteSpace(model.Line) &&
                    string.IsNullOrWhiteSpace(model.Group) &&
                    string.IsNullOrWhiteSpace(model.ClassName) &&
                    string.IsNullOrWhiteSpace(model.PlanWorkPlace) &&
                    string.IsNullOrWhiteSpace(model.ActualWorkPlace))
                {
                    // Initial load using NoReg only
                    var events = await weeklyWFHReportService.GetEventsSuperiorAsync(noreg);

                    if (events == null || events.Count == 0)
                    {
                        return NotFound("No events found for the current month and year.");
                    }

                    return Ok(events);
                }
                else
                {
                    // Filtered request
                    var events = await weeklyWFHReportService.GetFiltersAsync(
                        startDateString,  // Pass the formatted start date string
                        model.EmployeeName,
                        model.Directorate,
                        model.Division,
                        model.Department,
                        model.Section,
                        model.Line,
                        model.Group,
                        model.ClassName,
                        model.PlanWorkPlace,
                        model.ActualWorkPlace
                    );

                    if (events == null || events.Count == 0)
                    {
                        return NotFound("No events found for the given criteria.");
                    }

                    return Ok(events);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving events: {ex.Message}");
                return StatusCode(500, $"An error occurred while retrieving events: {ex.Message}");
            }
        }


        //[HttpGet("get-filters")]
        //public IActionResult GetFilters(string startDate)
        //{
        //    try
        //    {
        //        var events = weeklyWFHReportService.GetFilters(startDate); // Pass startDate to the service method

        //        if (events == null || events.Count == 0)
        //        {
        //            return NotFound("No events found for the given date range.");
        //        }

        //        return Ok(events);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error retrieving events: {ex.Message}");
        //        return StatusCode(500, $"An error occurred while retrieving events: {ex.Message}");
        //    }
        //}


        [HttpPost("getactives")]
        public async Task<DataSourceResult> GetActiveUsers([DataSourceRequest] DataSourceRequest request)
        {
            return await userService.GetActiveUsers().ToDataSourceResultAsync(request);
        }

        [HttpGet("download")]
        public IActionResult Download(string employeeName, DateTime dateFrom, DateTime dateTo, string listDirectorate, string listDivision, string listDepartment, string listSection, string listLine, string listGroup, string className, string noReg, string postCode)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postcode = ServiceProxy.UserClaim.PostCode;
            var name = ServiceProxy.UserClaim.Name;
            var safePostCode = System.Web.HttpUtility.HtmlEncode(postCode);
            var safeNoReg = System.Web.HttpUtility.HtmlEncode(noReg);

            if (employeeName == null)
            {
                employeeName = "";
            }
            List<WeeklyWFHDownloadStoredEntity> getData;

            if (ServiceProxy.UserClaim.Chief)
            {
                getData = weeklyWFHReportService.GetWeeklyWFHDownloadReports(dateFrom, dateTo, noreg, postcode).ToList();
            }
            else
            {
                // Assuming 'P' is used in place of `postcode` for non-chiefs.
                getData = weeklyWFHReportService.GetWeeklyWFHDownloadReports(dateFrom, dateTo, safeNoReg, safePostCode).ToList();
            }

            getData = getData
            .OrderByDescending(data => data.Name == name)
            .ThenBy(data => data.Name)
            .ToList();

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

            if (!string.IsNullOrEmpty(postCode))
            {
                getData = getData.Where(c => c.PostCode.Contains(postCode)).ToList();
            }

            var format = "dd/MM/yyyy";

            var listEmployeePlan = (from data in getData
                                    where data.Type == "Plan"
                                    group data by new { data.NoReg, data.Date } into dataGroup
                                    select new EmployeeTemporarySummary { NoReg = dataGroup.First().NoReg, Date = dataGroup.First().Date, WorkPlace = dataGroup.First().WorkPlace}
                                    ).ToList();

            var listEmployee = (from data in getData
                                group data by new { data.NoReg } into dataGroup
                                select new EmployeeSummary
                                {
                                    NoReg = dataGroup.First().NoReg,
                                    Name = dataGroup.First().Name,
                                    PlanActualPercent = dataGroup.Count() == 0 ? 0 : Convert.ToDecimal(dataGroup.Count(x => x.Type == "Actual" && listEmployeePlan.FirstOrDefault(y => y.NoReg == x.NoReg && y.Date == x.Date && y.WorkPlace == x.WorkPlace) != null) * 100) / (dataGroup.Count() / 2),
                                    PlanMinWFO = dataGroup.Count(x => x.Type == "Plan" && x.WorkPlace != "OFF" && x.WorkPlace != "OFF") == 0 ? 0 : Convert.ToDecimal(dataGroup.Count(x => (x.WorkPlace == "WFO" || x.WorkPlace == "WFF") && x.Type == "Plan") * 100 / dataGroup.Count(x => x.Type == "Plan" && x.WorkPlace != "OFF" && x.WorkPlace != "OFF")),
                                    ActualMinWFO = dataGroup.Count(x => x.Type == "Actual" && x.WorkPlace != "OFF" && x.WorkPlace != "OFF") == 0 ? 0 : Convert.ToDecimal(dataGroup.Count(x => (x.WorkPlace == "WFO" || x.WorkPlace == "WFF") && x.Type == "Actual") * 100 / dataGroup.Count(x => x.Type == "Actual" && x.WorkPlace != "OFF" && x.WorkPlace != "OFF")),
                                    PlanCountWorkDay = Convert.ToDecimal(dataGroup.Count(x => (x.WorkPlace == "WFO" || x.WorkPlace == "WFF") && x.Type == "Plan")),
                                    ActualCountWorkDay = Convert.ToDecimal(dataGroup.Count(x => (x.WorkPlace == "WFO" || x.WorkPlace == "WFF") && x.Type == "Actual")),
                                    PostName = dataGroup.First().PostName,
                                    JobName = dataGroup.First().JobName,
                                    Division = dataGroup.First().Division,
                                    Department = dataGroup.First().Department,
                                    Section = dataGroup.First().Section,
                                    Line = dataGroup.First().Line,
                                    Group = dataGroup.First().Group,
                                    Class = dataGroup.First().Class
                                }).ToList();

            var fileName = string.Format("Weekly WFH Report {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", dateFrom, dateTo);

            var dates = new List<DateTime>();

            for (var dt = dateFrom; dt <= dateTo; dt = dt.AddDays(1))
            {
                dates.Add(dt);
            }

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var detailSheet = package.Workbook.Worksheets.Add("Detail");
                    var detailCols = new List<string>{ "Noreg", "Name", "Date", "Type", "WorkPlace", "PostName", "JobName", "Division", "Department", "Section", "Line", "Group"};

                    //if(ServiceProxy.UserClaim.Chief)
                    //{
                    //    detailCols.Add("Class");
                    //}

                    for (var i = 1; i <= detailCols.Count; i++)
                    {
                        detailSheet.Cells[1, i].Value = detailCols[i - 1];
                        detailSheet.Cells[1, i].Style.Font.Bold = true;
                        detailSheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        detailSheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        detailSheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        detailSheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        detailSheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                    }

                    int idxCol = 1;
                    foreach (var data in getData)
                    {
                        idxCol = 1;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.NoReg;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Name;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Date.ToString(format);
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Type;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.WorkPlace;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.PostName;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.JobName;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Division;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Department;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Section;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Line;
                        detailSheet.Cells[rowIndex, idxCol++].Value = data.Group;
                        
                        //if (ServiceProxy.UserClaim.Chief)
                        //{
                        //    detailSheet.Cells[rowIndex, idxCol++].Value = data.Class;
                        //}
                        
                        rowIndex++;
                    }

                    int minWFOCount = 1;

                    Config minWFOCon = ConfigService.GetConfig("WeeklyWFHPlanning.MinWFOCount");
                    if (minWFOCon != null)
                    {
                        minWFOCount = Convert.ToInt32(minWFOCon.ConfigValue);
                    }

                    double minWFOCountDays = Convert.ToInt32(((dateTo - dateFrom).TotalDays + 1) / 7) * minWFOCount;

                    rowIndex = 2;
                    idxCol = 1;
                    var summarySheet = package.Workbook.Worksheets.Add("Summary");
                    var summaryCols = new List<string> { "Noreg", "Name", "StartDate", "EndDate", "Percentage Plan VS Actual", "Percentage Min WFO (Plan)", "Percentage Min WFO (Actual)", "PostName", "JobName", "Division", "Department", "Section", "Line", "Group" };

                    //if (ServiceProxy.UserClaim.Chief)
                    //{
                    //    summaryCols.Add("Class");
                    //}

                    for (var i = 1; i <= summaryCols.Count; i++)
                    {
                        summarySheet.Cells[1, i].Value = summaryCols[i - 1];
                        summarySheet.Cells[1, i].Style.Font.Bold = true;
                        summarySheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        summarySheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        summarySheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        summarySheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        summarySheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                    }

                    foreach (var employee in listEmployee)
                    {
                        idxCol = 1;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.NoReg;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Name;
                        summarySheet.Cells[rowIndex, idxCol++].Value = dateFrom.ToString(format);
                        summarySheet.Cells[rowIndex, idxCol++].Value = dateTo.ToString(format);
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.PlanActualPercent.ToString("#.## ") + " %";
                        summarySheet.Cells[rowIndex, idxCol++].Value = (employee.PlanMinWFO != 0 ? employee.PlanMinWFO.ToString("#.##") : "0") + " %";
                        if (employee.PlanCountWorkDay < Convert.ToInt32(minWFOCountDays))
                        {
                            summarySheet.Cells[rowIndex, idxCol - 1].Style.Font.Color.SetColor(Color.Red);
                        }
                        summarySheet.Cells[rowIndex, idxCol++].Value = (employee.ActualMinWFO != 0 ? employee.ActualMinWFO.ToString("#.##") : "0") + " %";
                        if (employee.ActualCountWorkDay < Convert.ToInt32(minWFOCountDays))
                        {
                            summarySheet.Cells[rowIndex, idxCol - 1].Style.Font.Color.SetColor(Color.Red);
                        }
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.PostName;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.JobName;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Division;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Department;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Section;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Line;
                        summarySheet.Cells[rowIndex, idxCol++].Value = employee.Group;
                        

                        //if (ServiceProxy.UserClaim.Chief)
                        //{
                        //    summarySheet.Cells[rowIndex, idxCol++].Value = employee.Class;
                        //}

                        rowIndex++;
                    }

                    package.SaveAs(ms);

                    ms.Close();
                }
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    

    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class WeeklyWFHReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            ViewBag.directorateData = GetDirectorateTree();

            ViewBag.workPlaceData = GetWorkPlace();

            //var now = DateTime.Now;
            //
            //Config testerDateCat = ConfigService.GetConfig("Others.TesterDate");
            //if (testerDateCat != null && testerDateCat.ConfigText == "True")
            //{
            //    now = DateTime.Parse(testerDateCat.ConfigValue);
            //}
            //
            //int gap = 14;
            //
            //WFHGeneratePlanningDateWeekly pdw = ServiceProxy.GetService<WeeklyWFHPlanningService>().GetWeeklyWFHPlanningByDate(now, gap, true);
            //var noreg = ServiceProxy.UserClaim.NoReg;
            //var postCode = ServiceProxy.UserClaim.PostCode;
            //
            //ViewBag.weeklyWFHReport = ServiceProxy.GetService<WeeklyWFHReportService>().GetWeeklyWFHReport(pdw.StartDate, pdw.EndDate, noreg, postCode);
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

        public List<DropDownListItem> GetWorkPlace()
        {
            // Create new TreeViewItemModel list object.
            var listDropDownList = new List<DropDownListItem>();

            listDropDownList.Add(new DropDownListItem
            {
                Value = "WFH",
                Text = "WFH"
            });

            listDropDownList.Add(new DropDownListItem
            {
                Value = "WFO",
                Text = "WFO"
            });

            listDropDownList.Add(new DropDownListItem
            {
                Value = "WFF",
                Text = "WFF"
            });

            listDropDownList.Add(new DropDownListItem
            {
                Value = "ABS",
                Text = "Absent"
            });

            listDropDownList.Add(new DropDownListItem
            {
                Value = "OFF",
                Text = "Off"
            });

            listDropDownList = listDropDownList.OrderBy(x => x.Text).ToList();

            // Return partial view with given view model.
            return listDropDownList;
        }

    }
    #endregion
}
