using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.DomainServices;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common.Utility;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using System.Drawing;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Daily and monthly time monitoring report API Manager
    /// </summary>
    [Route("api/Attandance-Incentive")]
    public class AttandanceIncentiveApiController : ApiControllerBase
    {
        #region Domain Services
        protected EmployeeProfileService employeeProfileService => ServiceProxy.GetService<EmployeeProfileService>();
        /// <summary>
        /// Monitoring report service object
        /// </summary>
        public MonitoringReportAllService MonitoringReportService => ServiceProxy.GetService<MonitoringReportAllService>();

        /// <summary>
        /// Master data management service object
        /// </summary>
        public MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Time management service object
        /// </summary>
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        /// <summary>
        /// Get list of monitoring
        /// </summary>
        /// <remarks>
        /// Get list of monitoring
        /// </remarks>
        /// <param name="keyDate">Key Date</param>
        /// <param name="checkMonth">Check Month</param>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([FromForm] string noreg, [FromForm]DateTime startDate, [FromForm]DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg);

            request.Filters = request.Filters ?? new List<IFilterDescriptor>();

            request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));

            return ServiceProxy.GetTableValuedDataSourceResult<TimeManagementStoredEntity>(request, new { organizationStructure.PostCode, orgCode = organizationStructure.OrgCode, startDate, endDate });
        }

        /// <summary>
        /// Get daily monitoring
        /// </summary>
        /// <remarks>
        /// Get daily monitoring
        /// </remarks>
        [HttpPost("get-daily-monitoring")]
        public DataSourceResult GetDailyMonitoring([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var organization = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organization.OrgCode;
            var keyDate = DateTime.Now;
            var useLinkedServer = ConfigService.GetConfigValue("App.UseLinkedServer", false, false);

            if (!ServiceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            if (useLinkedServer)
            {
                var linkedServer = ConfigService.GetConfigValue("App.LinkedServer", string.Empty, false);
                var spldLinkedServer = ConfigService.GetConfigValue("App.SpldLinkedServer", string.Empty, false);
                var keyDateStr = keyDate.ToString("yyyyMMdd");

                return ServiceProxy.GetQueryDataSourceResult<TimeMonitoringSubordinateStoredEntity>(string.Format(@"
                    SELECT
				        eo.NoReg,
				        eo.Name,
				        eo.PostName,
				        eo.JobLevel,
				        oa.EmployeeSubgroup,
				        gc.Name AS EmployeeSubgroupText,
                        div.ObjectText AS Division,
                        dep.ObjectText AS Department,
                        sec.ObjectText AS Section,
                        grp.ObjectText AS [Group],
                        lin.ObjectText AS Line,
                        ISNULL(MIN(proxy.ProxyTime),pt.ProxyIn) AS ProxyIn,
                        ISNULL(MAX(proxy.ProxyTime),pt.ProxyOut) AS ProxyOut,
				        CASE WHEN COUNT(pt.ProxyIn)+COUNT(pt.ProxyOut)=0
							THEN SUM(IIF(proxy.Staff_Number IS NULL, 0, 1))
						ELSE COUNT(pt.ProxyIn)+(CASE WHEN pt.ProxyOut IS NOT NULL AND pt.ProxyOut=pt.ProxyIn THEN 0 ELSE 1 END) END AS TotalProxy
			        FROM dbo.SF_GET_EMPLOYEE_BY_ORG(@orgCode, @keyDate) eo
                    LEFT JOIN dbo.MDM_ORGANIZATIONAL_ASSIGNMENT oa ON oa.NoReg = eo.NoReg AND @keyDate BETWEEN oa.StartDate AND oa.EndDate
                    LEFT JOIN dbo.TB_M_GENERAL_CATEGORY gc ON gc.Code = 'es.' + oa.EmployeeSubgroup
			        LEFT JOIN dbo.MDM_POSITION_JOB_REL pjr ON pjr.PostCode = @postCode AND @keyDate BETWEEN pjr.StartDate AND pjr.EndDate
			        LEFT JOIN dbo.MDM_JOB_CHIEF jc ON jc.JobCode = pjr.JobCode
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE div ON div.OrgCode = eo.OrgCode AND div.ObjectDescription = 'Division'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE dep ON dep.OrgCode = eo.OrgCode AND dep.ObjectDescription = 'Department'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE sec ON sec.OrgCode = eo.OrgCode AND sec.ObjectDescription = 'Section'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE grp ON grp.OrgCode = eo.OrgCode AND grp.ObjectDescription = 'Group'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE lin ON lin.OrgCode = eo.OrgCode AND lin.ObjectDescription = 'Line'
			        LEFT JOIN (
				        SELECT
                            Staff_Number COLLATE DATABASE_DEFAULT AS Staff_Number,
						    CAST(Tr_Date AS DATE) AS ProxyDate,
						    CAST(Tr_Date + ' ' + STUFF(STUFF(Tr_Time, 5, 0, ':'), 3, 0, ':') AS DATETIME) AS ProxyTime
                        FROM OPENQUERY([{0}], 'SELECT * FROM matrix.dbo.Data_Proxy WHERE Staff_Number <> '''' AND Tr_Date = ''{1}''')
                        UNION
                        SELECT
                            Staff_Number COLLATE DATABASE_DEFAULT AS Staff_Number,
						    CAST(Tr_Date AS DATE) AS ProxyDate,
						    CAST(Tr_Date + ' ' + STUFF(STUFF(Tr_Time, 5, 0, ':'), 3, 0, ':') AS DATETIME) AS ProxyTime
                        FROM OPENQUERY([{2}], 'SELECT * FROM Matrix.dbo.Data_Proxy_SPLD WHERE Staff_Number <> '''' AND Tr_Date = ''{1}''')
                        UNION
                        SELECT 
                        NoReg as Staff_Number,
                        AccessDate AS ProxyDate,
                        AccessDateTime As ProxyTime
                        FROM TB_R_ATTENDANCE_RECORD
                        WHERE Noreg <> '' AND AccessDate = @keyDate
			        ) proxy ON proxy.Staff_Number = eo.NoReg
                    LEFT JOIN TB_R_PROXY_TIME pt ON pt.NoReg=eo.NoReg AND CONVERT(varchar(10),pt.WorkingDate,120)=CONVERT(varchar(10), @keyDate, 120)
			        WHERE eo.Staffing = 100 AND (eo.JobLevel > jc.JobLevel OR eo.PostCode = @postCode)
			        GROUP BY eo.NoReg, eo.Name, eo.PostName, eo.JobLevel, oa.EmployeeSubgroup, gc.Name, div.ObjectText, 
                    dep.ObjectText, sec.ObjectText, grp.ObjectText, lin.ObjectText, pt.ProxyIn, pt.ProxyOut
                ", linkedServer, keyDateStr, spldLinkedServer), request, new { keyDate, orgCode, postCode });
            }
            else
            {
                return ServiceProxy.GetTableValuedDataSourceResult<TimeMonitoringSubordinateStoredEntity>(request, new { postCode, orgCode, keyDate });
            }
        }
        [HttpPost("get-employee")]
        public async Task<DataSourceResult> GetEmployeeProfile([DataSourceRequest] DataSourceRequest request)
        {
            var result = await employeeProfileService.getEmployee().ToDataSourceResultAsync(request);

            return result;
        }
        [HttpPost("get-employee-actual")]
        public async Task<DataSourceResult> GetEmployeeProfileActual([DataSourceRequest] DataSourceRequest request)
        {
            var result = await employeeProfileService.getEmployeeActual().ToDataSourceResultAsync(request);

            return result;
        }

        //[HttpGet("download-daily-monitoring")]
        //public async Task<IActionResult> DownloadDailyMonitoring()
        //{
        //    return await Task.Run(() =>
        //    {
        //        var request = new DataSourceRequest
        //        {
        //            Sorts = new List<SortDescriptor>
        //            {
        //                new SortDescriptor("Name", ListSortDirection.Ascending)
        //            }
        //        };
        //        var dataSourceResult = GetDailyMonitoring(request);
        //        var output = dataSourceResult.Data.OfType<TimeMonitoringSubordinateStoredEntity>();
        //        var fileName = string.Format("DAILY-REPORT-{0:ddMMyyyy}.xlsx", DateTime.Now);

        //        using (var ms = new MemoryStream())
        //        {
        //            using (var package = new ExcelPackage())
        //            {
        //                int rowIndex = 2;
        //                var sheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(fileName));

        //                var cols = new[] { "Noreg", "Name", "Position", "Class", "Division", "Department", "Section", "Line", "Group", "Proxy In", "Proxy Out", "Total Proxy" };

        //                for (var i = 1; i <= cols.Length; i++)
        //                {
        //                    sheet.Cells[1, i].Value = cols[i - 1];

        //                    var style = sheet.Cells[1, i].Style;

        //                    style.Font.Bold = true;
        //                    style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        //                    style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                    style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                    style.Fill.BackgroundColor.SetColor(Color.Black);
        //                    style.Font.Color.SetColor(Color.White);
        //                }

        //                foreach (var item in output)
        //                {
        //                    sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
        //                    sheet.Cells[rowIndex, 2].Value = item.Name;
        //                    sheet.Cells[rowIndex, 3].Value = item.PostName;
        //                    sheet.Cells[rowIndex, 4].Value = item.EmployeeSubgroupText;
        //                    sheet.Cells[rowIndex, 5].Value = item.Division;
        //                    sheet.Cells[rowIndex, 6].Value = item.Department;
        //                    sheet.Cells[rowIndex, 7].Value = item.Section;
        //                    sheet.Cells[rowIndex, 8].Value = item.Line;
        //                    sheet.Cells[rowIndex, 9].Value = item.Group;
        //                    sheet.Cells[rowIndex, 10].Value = item.ProxyIn.HasValue ? item.ProxyIn.Value.ToString("HH:mm") : string.Empty;
        //                    sheet.Cells[rowIndex, 10].Style.QuotePrefix = true;
        //                    sheet.Cells[rowIndex, 11].Value = item.ProxyOut.HasValue ? item.ProxyOut.Value.ToString("HH:mm") : string.Empty;
        //                    sheet.Cells[rowIndex, 11].Style.QuotePrefix = true;
        //                    sheet.Cells[rowIndex, 12].Value = item.TotalProxy;

        //                    for (var i = 1; i <= cols.Length; i++)
        //                    {
        //                        sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        //                    }

        //                    rowIndex++;
        //                }

        //                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        //                package.SaveAs(ms);
        //            }

        //            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //        }
        //    });
        //}

        /// <summary>
        /// Get monthly monitoring
        /// </summary>
        /// <remarks>
        /// Get monthly monitoring
        /// </remarks>
        [HttpPost("get-abnormality")]
        public DataSourceResult GetAbnormality([FromForm] DateTime startDate, [FromForm] DateTime endDate,[DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            var configStart = ConfigService.GetConfig("Abnormality.StartDate");
            var configEnd = ConfigService.GetConfig("Abnormality.EndDate");

            var dtStart = Convert.ToDateTime(configStart.ConfigValue);
            var dtEnd = Convert.ToDateTime(configEnd.ConfigValue);

            return ServiceProxy.GetTableValuedDataSourceResult<AbnormalityStoredEntity>(request, new { noreg, startDate, endDate });
        }


        /// <summary>
        /// Get monthly monitoring
        /// </summary>
        /// <remarks>
        /// Get monthly monitoring
        /// </remarks>
        [HttpPost("get-monthly-monitoring")]
        public DataSourceResult GetMonthlymonitoring([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var organization = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organization.OrgCode;

            if (!ServiceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            return ServiceProxy.GetTableValuedDataSourceResult<TimeMonitoringSubordinateMonthlyStoredEntity>(request, new { postCode, orgCode, startDate, endDate });
        }

        /// <summary>
        /// Get list of proxy details by noreg and key date
        /// </summary>
        /// <remarks>
        /// Get list of proxy details by noreg and key date
        /// </remarks>
        [HttpPost("get-proxy-details")]
        public DataSourceResult GetProxyDetails([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            var now = DateTime.Now;

            return MonitoringReportService.GetMatrixProxyDetails(noreg, now).ToDataSourceResult(request);
        }

        /// <summary>
        /// Get time monitoring summary
        /// </summary>
        /// <remarks>
        /// Get time monitoring summary
        /// </remarks>
        [HttpGet("summary")]
        public TimeManagementSummaryStoredEntity GetSummary(DateTime startDate, DateTime endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var summary = MonitoringReportService.GetSummary(noreg,postCode, organizationStructure.OrgCode, startDate, endDate);


            var configStart = ConfigService.GetConfig("Abnormality.StartDate");
            var configEnd = ConfigService.GetConfig("Abnormality.EndDate");
            
            var dtStart = Convert.ToDateTime(configStart.ConfigValue);
            var dtEnd = Convert.ToDateTime(configEnd.ConfigValue);
            
            var result = ServiceProxy.GetTableValuedDataSourceResult<AbnormalityStoredEntity>(request, new { noreg, dtStart, dtEnd });

            var totalAbnormality = 0;

            foreach (AbnormalityStoredEntity data in result.Data) {
                totalAbnormality += data.TotalAbnormality;
            }

            summary.Abnormality = totalAbnormality;

            return summary;
        }

        /// <summary>
        /// Get time monitoring absence summary
        /// </summary>
        /// <remarks>
        /// Get time monitoring absence summary
        /// </remarks>
        [HttpPost("absence-summary")]
        public DataSourceResult GetAbsenceSummary([FromForm]string noreg, [FromForm]DateTime startDate, [FromForm]DateTime endDate, [FromForm]string category, [FromForm]bool showAll, [DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<AbsenceSummaryStoredEntity>(request, new { noreg, startDate, endDate, category, showAll });
        }
        [HttpPost("absence-summaryAll")]
        public DataSourceResult GetAbsenceSummaryAll([FromForm] DateTime startDate, [FromForm] DateTime endDate, [FromForm] string category, [FromForm] bool showAll, [DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<AbsenceSummaryAllStoredEntity>(request, new {startDate, endDate, category, showAll });
        }
        /// <summary>
        /// Get time monitoring absence summary details
        /// </summary>
        /// <remarks>
        /// Get time monitoring absence summary details
        /// </remarks>
        [HttpPost("absence-summary-details")]
        public DataSourceResult GetAbsenceSummaryDetails([FromForm]string noreg, [FromForm]DateTime startDate, [FromForm]DateTime endDate, [FromForm]string category, [DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<AbsenceSummaryDetailsStoredEntity>(request, new { noreg, startDate, endDate, category });
        }

        /// <summary>
        /// Get time management details
        /// </summary>
        /// <remarks>
        /// Get time management details
        /// </remarks>
        [HttpGet("time-management-details")]
        public IActionResult GetTimeManagementDetails(string filter, DateTime startDate, DateTime endDate, string employeeSubgroup, string workContract)
        {
            var now = DateTime.Now;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var structure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var jobCode = structure.JobCode;
            var orgCode = structure.OrgCode;

            if (!ServiceProxy.UserClaim.Chief)
            {
                filter = noreg;
            }

            var data = TimeManagementService.GetTimeManagementDetails(jobCode, orgCode, startDate, endDate, filter, employeeSubgroup, workContract);

            return ExportToXlsx(data, string.Format("MONTHLY-REPORT-{0:ddMMyyyy}.xlsx", now), new[] { "Id" }, callback: (column, value, cell) =>
            {
                if (column == "WorkingTimeIn" || column == "WorkingTimeOut" || column == "WorkingDate")
                {
                    if (column != "WorkingDate")
                    {
                        cell.Value = value == null
                            ? string.Empty
                            : (value is DateTime ? ((DateTime)value).ToString("HH:mm") : value.ToString());
                    }

                    cell.Style.QuotePrefix = value != null;
                }
            });
        }

        [HttpPost("get-dashboard-unique-column-values")]
        public IActionResult GetUniqueColumnValuesDashboard(string field)
        {
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-directorate")]
        public IActionResult GetUniqueColumnValuesDirectorate()
        {
            string field = "Directorate";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-division")]
        public IActionResult GetUniqueColumnValuesDivision()
        {
            string field = "Divisi";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-department")]
        public IActionResult GetUniqueColumnValuesDepartment()
        {
            string field = "Department";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-section")]
        public IActionResult GetUniqueColumnValuesSection()
        {
            string field = "JobName";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-noreg")]
        public IActionResult GetUniqueColumnValuesNoReg()
        {
            string field = "NoReg";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-name")]
        public IActionResult GetUniqueColumnValuesName()
        {
            string field = "Name";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-status")]
        public IActionResult GetUniqueColumnValuesStatus()
        {
            string field = "Expr1";
            var uniqueValues = MonitoringReportService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class AttandanceIncentiveController : MvcControllerBase
    {
        protected AbsenceService AbsenceService => ServiceProxy.GetService<AbsenceService>();

        public IActionResult Index(string mode = "daily")
        {
            ViewBag.Mode = !ObjectHelper.IsIn(mode, new[] { "daily", "monthly" }) ? "daily" : mode;

            return View();
        }
        protected ProfileService ProfileService { get { return ServiceProxy.GetService<ProfileService>(); } }

      /*  public IActionResult BonusDeduction()
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, 1, 1);
            var endDate = new DateTime(now.Year, 12, 31);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return GetIncentiveReport("bonus-deduction");
        }*/

   /*     public IActionResult AttendanceIncentive()
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year - 1, 5, 1);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = startDate.AddYears(1).AddDays(-1);

            return GetIncentiveReport("attendance-incentive");
        }
*/
       
        public IActionResult IncentiveReport(string category)
        {
            var categories = AbsenceService.GetAbsencesBySummaryCategory(category);
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, 1, 1);
            var endDate = new DateTime(now.Year, 12, 31);

            ViewBag.directorateData = GetDirectorateTree();
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Category = category;
            ViewBag.Categories = categories;


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