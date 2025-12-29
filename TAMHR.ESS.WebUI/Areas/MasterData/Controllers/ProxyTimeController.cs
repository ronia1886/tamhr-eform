using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.IO;
using OfficeOpenXml;
using System.Data;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Proxy Time API Manager
    /// </summary>
    [Route("api/proxy-time")]
    [Permission(PermissionKey.ManageProxyTime)]
    public class ProxyTimeApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Proxy time service object
        /// </summary>
        public ProxyTimeService ProxyTimeService => ServiceProxy.GetService<ProxyTimeService>();
        public DailyWorkScheduleService DailyWorkScheduleService => ServiceProxy.GetService<DailyWorkScheduleService>();
        #endregion

        /// <summary>
        /// Get list of proxy time
        /// </summary>
        /// <remarks>
        /// Get list of proxy time
        /// </remarks>
        /// <returns>List of Proxy Time</returns>
        [HttpGet]
        public IEnumerable<Domain.TimeManagementView> Gets() => ProxyTimeService.Gets();

        /// <summary>
        /// Get list of proxy time
        /// </summary>
        /// <remarks>
        /// Get list of proxy time
        /// </remarks>
        /// <returns>List of Proxy Time</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            if(request.Filters.Count == 0)
            {
                return ServiceProxy.GetDataSourceResult<TimeManagementView>(request,new {WorkingDate=DateTime.Now.Date});
            }
            else
            {
                return ServiceProxy.GetDataSourceResult<TimeManagementView>(request);
            }
            
        }

        /// <summary>
        /// Get list of proxy time update histories
        /// </summary>
        /// <remarks>
        /// Get list of proxy time update histories
        /// </remarks>
        /// <returns>List of Proxy Time</returns>
        [HttpPost("get-histories")]
        public async Task<DataSourceResult> GetHistoriesFromPosts([FromForm] string noreg, [FromForm] DateTime date, [DataSourceRequest] DataSourceRequest request)
        {
            return await ProxyTimeService.GetHistoriesQuery(noreg, date).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new proxy time
        /// </summary>
        /// <remarks>
        /// Create new proxy time
        /// </remarks>
        /// <param name="proxyTime">Proxy Time Object</param>
        /// <returns>Created Proxy Time Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Domain.TimeManagement proxyTime)
        {
            ProxyTimeService.Upsert(ServiceProxy.UserClaim.NoReg, proxyTime);

            return CreatedAtAction("Get", new { id = proxyTime.Id });
        }

        /// <summary>
        /// Update proxy time
        /// </summary>
        /// <remarks>
        /// Update proxy time
        /// </remarks>
        /// <param name="proxyTime">Proxy Time Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody] Domain.TimeManagement proxyTime)
        {
            ProxyTimeService.Upsert(ServiceProxy.UserClaim.NoReg, proxyTime);

            return NoContent();
        }

        /// <summary>
        /// Delete proxy time history by id
        /// </summary>
        /// <remarks>
        /// Delete proxy time history by id
        /// </remarks>
        /// <param name="id">Proxy Time History Id</param>
        /// <returns>No Content</returns>
        [HttpDelete("delete-history")]
        public IActionResult DeleteHistory([FromForm]Guid id)
        {
            ProxyTimeService.DeleteHistory(id);

            return NoContent();
        }

        /// <summary>
        /// Download data
        /// </summary>
        /// <remarks>
        /// Download data
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download")]
        public IActionResult Download(string noreg, string name, DateTime? sd, DateTime? ed)
        {
            var data = ProxyTimeService.GetQuery(noreg, name, sd, ed).ToList();

            return ExportToXlsx(data, $"ProxyTime_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx", excludes: new[] { "NormalTimeIn", "NormalTimeOut","Id", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" }, callback: (column, value, range) =>
            {
                if (column == "WorkingTimeIn" || column == "WorkingTimeOut" || column == "WorkingDate")
                {
                    if (column != "WorkingDate")
                    {
                        range.Value = value == null
                            ? string.Empty
                            : (value is DateTime ? ((DateTime)value).ToString("HH:mm") : value.ToString());
                    }

                    range.Style.QuotePrefix = value != null;
                }
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];
            //var spklCategories = ConfigService.GetGeneralCategories("CategorySPKL").ToDictionary(x => x.Name);
            var dailyWorkSchedules = DailyWorkScheduleService.Gets();
            var now = DateTime.Now.Date;
            //var canUploadBackDate = AclHelper.HasPermission("Core.CanUploadSpklBackDate");
            var totalSuccess = 0;
            var totalUpload = 0;
            var messages = new List<string>();
            var listInclude = new List<string>() { "1NS8", "1NJ8" };

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    using (var workSheet = package.Workbook.Worksheets.FirstOrDefault())
                    {
                        if (workSheet == null) return NoContent();

                        var totalRows = workSheet.Dimension.Rows;
                        var initialStart = 2;
                        var rowStart = initialStart;
                        var dt = new DataTable();

                        dt.Columns.AddRange(new[] {
                            new DataColumn("NoReg", typeof(string)),
                            new DataColumn("WorkingDate", typeof(DateTime)),
                            new DataColumn("WorkingTimeIn", typeof(DateTime)),
                            new DataColumn("WorkingTimeOut", typeof(DateTime)),
                            new DataColumn("ShiftCode", typeof(string)),
                            new DataColumn("AbsentStatus", typeof(string)),
                            new DataColumn("Description", typeof(string))
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            var noreg = workSheet.Cells[rowStart, 1].Value.ToString();
                            DateTime workingTimeIn;
                            DateTime workingTimeOut;
                            var workingDate = workSheet.Cells[rowStart, 2].Text;
                           
                            var workingDateTime =  DateTime.ParseExact(workingDate, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                            var shiftCode = workSheet.Cells[rowStart, 5].Text;
                            var dailyWorkSchedule = dailyWorkSchedules.FirstOrDefault(x => x.ShiftCode == shiftCode);
                            var normalTimeInText = dailyWorkSchedule.NormalTimeINText;
                            var normalTimeOutText = dailyWorkSchedule.NormalTimeOutText;

                            var NormalTimeIn = DateTime.ParseExact(workingDate + " " + normalTimeInText, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                            var NormalTimeOut = DateTime.ParseExact(workingDate + " " + normalTimeOutText, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);

                            if (dailyWorkSchedule.NormalTimeOut < dailyWorkSchedule.NormalTimeIN)
                            {
                                NormalTimeOut = NormalTimeOut.AddDays(1);
                            }

                            try
                            {
                                workingTimeIn = DateTime.ParseExact(workingDate + " " + workSheet.Cells[rowStart, 3].Text, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            try
                            {
                                workingTimeOut = DateTime.ParseExact(workingDate + " " + workSheet.Cells[rowStart, 4].Text, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            var minNormalTimeOut = workingTimeIn.AddHours(8).AddMinutes(45);
                            if(shiftCode == "1NJ8")
                            {
                                minNormalTimeOut = workingTimeIn.AddHours(9);
                            }
                            //    var workSchedule = dailyWorkSchedules[shiftCode];
                            //    var NormalTimeIn = workingDate.

                            if((workingTimeIn < NormalTimeIn && listInclude.Contains(shiftCode)) || !listInclude.Contains(shiftCode))
                            {
                                minNormalTimeOut = NormalTimeOut;
                            }

                            if (workingTimeOut < minNormalTimeOut )
                            {
                                messages.Add(string.Format("Row {0}: WorkingTimeOut Cannot Less from {1}", rowStart, minNormalTimeOut));
                                rowStart++;
                                continue;
                            }


                            //    int overtimeBreak;
                            //    TimeSpan overtimeIn;
                            //    TimeSpan overtimeOut;

                            //    try
                            //    {
                            //        overtimeBreak = Math.Abs(int.Parse(workSheet.Cells[rowStart, 6].Value.ToString()));
                            //        overtimeIn = TimeSpan.Parse(workSheet.Cells[rowStart, 4].Value.ToString());
                            //        overtimeOut = TimeSpan.Parse(workSheet.Cells[rowStart, 5].Value.ToString());
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                            //        rowStart++;
                            //        continue;
                            //    }

                            //    var overtimeInDate = overtimeDate.Add(overtimeIn);
                            //    var overtimeOutDate = overtimeIn > overtimeOut ? overtimeDate.AddDays(1).Add(overtimeOut) : overtimeDate.Add(overtimeOut);

                            //    if (overtimeOutDate < overtimeInDate)
                            //    {
                            //        messages.Add(string.Format("Row {0}: SPKL Out is lower than SPKL In", rowStart));
                            //        rowStart++;
                            //        continue;
                            //    }

                            //    var duration = ServiceHelper.CalculateDuration((decimal)(overtimeOutDate - overtimeInDate).TotalMinutes - overtimeBreak);
                            //    var category = (workSheet.Cells[rowStart, 7].Value ?? string.Empty).ToString();

                            //    if (string.IsNullOrEmpty(category))
                            //    {
                            //        messages.Add(string.Format("Row {0}: SPKL Category is empty", rowStart));
                            //        rowStart++;
                            //        continue;
                            //    }

                            //    if (!spklCategories.ContainsKey(category))
                            //    {
                            //        messages.Add(string.Format("Row {0}: SPKL Category is not in list", rowStart));
                            //        rowStart++;
                            //        continue;
                            //    }

                            //    var categoryCode = spklCategories.ContainsKey(category) ? spklCategories[category].Code : string.Empty;
                            //    var reason = workSheet.Cells[rowStart, 8].Text;

                            //    if (string.IsNullOrEmpty(reason))
                            //    {
                            //        messages.Add(string.Format("Row {0}: SPKL Reason is empty", rowStart));
                            //        rowStart++;
                            //        continue;
                            //    }

                            //    var row = dt.NewRow();
                            //    row.ItemArray = new object[] {
                            //        noreg,
                            //        overtimeDate,
                            //        overtimeBreak,
                            //        overtimeInDate,
                            //        overtimeOutDate,
                            //        duration,
                            //        categoryCode,
                            //        reason
                            //    };

                            // dt.Rows.Add(row);

                            rowStart++;
                                totalSuccess++;
                            //}

                            totalUpload = rowStart - initialStart;

                            //SpklService.UploadSpkl(actor, postCode, documentApprovalId, tempId, dt);
                        }
                    }
                }

                return Ok(new { TotalUpload = totalUpload, TotalSuccess = totalSuccess, TotalFailed = totalUpload - totalSuccess, Messages = messages });
            }
        }

        /// <summary>
        /// Download template
        /// </summary>
        /// <remarks>
        /// Download template
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<Domain.TimeManagement>($"{cleanControllerName}", new[] { "Id", "NormalTimeIn", "NormalTimeOut", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" });
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("merge")]
        public async Task<IActionResult> Merge()
        {
            var keys = new[] { "NoReg", "WorkingDate" };
            var noreg = ServiceProxy.UserClaim.NoReg;
            var columns = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "WorkingDate", typeof(DateTime?) },
                { "WorkingTimeIn", typeof(DateTime?) },
                { "WorkingTimeOut", typeof(DateTime?) },
                { "ShiftCode", typeof(string) },
                { "AbsentStatus", typeof(string) },
                { "Description", typeof(string) }
            };

            await UploadAndMergeAsync<Domain.TimeManagement>(Request.Form.Files.FirstOrDefault(), columns, keys, callback: (command, tableName) =>
            {
                command.CommandText = $"INSERT INTO dbo.TB_M_TIME_MANAGEMENT_HISTORY(NoReg, WorkingDate, WorkingTimeIn, WorkingTimeOut, PresenceCode, ShiftCode, Description, CreatedBy, CreatedOn) SELECT t.NoReg, t.WorkingDate, t.WorkingTimeIn, t.WorkingTimeOut, t.AbsentStatus, t.ShiftCode, t.Description, '{noreg}' AS CreatedBy, GETDATE() AS CreatedOn FROM {tableName} t;";
                command.ExecuteNonQuery();

                command.CommandText = string.Format(@"
                    MERGE INTO dbo.TB_R_TIME_MANAGEMENT AS TARGET
                    USING (
                        SELECT
                            t.Id,
                            CONVERT(DATETIME, CONVERT(CHAR(8), it.WorkingDate, 112) + ' ' + CONVERT(CHAR(8), ds.NormalTimeIN, 108)) AS NormalTimeIn,
                            CONVERT(DATETIME, IIF(ds.NormalTimeIN > ds.NormalTimeOut, CONVERT(CHAR(8), DATEADD(DAY, 1, it.WorkingDate), 112), CONVERT(CHAR(8), it.WorkingDate, 112)) + ' ' + CONVERT(CHAR(8), ds.NormalTimeOUT), 108) AS NormalTimeOut,
                            		case
		                    when Datediff(Minute,iif(it.WorkingTimeIn <= t.NormalTimeIn,t.NormalTimeIn,it.WorkingTimeIn),it.WorkingTimeOut) < 0 then null
		                    when it.ShiftCode in ('1NS8','1NJ8') then Datediff(Minute,iif(it.WorkingTimeIn <= t.NormalTimeIn,t.NormalTimeIn,it.WorkingTimeIn),it.WorkingTimeOut)
		                    else Datediff(Minute,it.WorkingTimeIn,it.WorkingTimeOut)
		                    End
		                    As WorkingHour
                        FROM {0} it
                        LEFT JOIN dbo.TB_R_TIME_MANAGEMENT t ON t.NoReg = it.NoReg AND CAST(t.WorkingDate AS DATE) = it.WorkingDate
                        LEFT JOIN dbo.TB_M_DAILY_WORK_SCHEDULE ds ON ds.ShiftCode = it.ShiftCode
                        WHERE t.Id IS NOT NULL
                    ) AS SOURCE ON SOURCE.Id = TARGET.Id
                    WHEN MATCHED THEN UPDATE SET
                        TARGET.NormalTimeIn = SOURCE.NormalTimeIn,
                        TARGET.NormalTimeOut = SOURCE.NormalTimeOut,
                        TARGET.WorkHour = SOURCE.WorkingHour;
                ", tableName, noreg);
                command.ExecuteNonQuery();

                command.CommandText = string.Format(@"
                    MERGE INTO dbo.TB_R_TIME_MANAGEMENT_EMP_WORK_SCHEDULE_SUBSTITUTE AS TARGET
                    USING (
                        SELECT it.*, t.ShiftCode AS OldShiftCode FROM {0} it
                        LEFT JOIN dbo.TB_R_TIME_MANAGEMENT t ON t.NoReg = it.NoReg AND CAST(t.WorkingDate AS DATE) = it.WorkingDate
                        WHERE t.ShiftCode <> it.ShiftCode
                    ) AS SOURCE ON SOURCE.NoReg = TARGET.NoReg AND SOURCE.WorkingDate = TARGET.Date
                    WHEN MATCHED THEN UPDATE SET
                        TARGET.ShiftCodeUpdate = SOURCE.ShiftCode
                    WHEN NOT MATCHED THEN
                        INSERT(NoReg, Date, ShiftCode, ShiftCodeUpdate, CreatedBy, CreatedOn)
                        VALUES(SOURCE.NoReg, SOURCE.WorkingDate, SOURCE.OldShiftCode, SOURCE.ShiftCode, '{1}', GETDATE());
                ", tableName, noreg);
                command.ExecuteNonQuery();
            },
            valueCallback: (column, range, idx, value) =>
            {
                if (column.ColumnName == "WorkingTimeIn" || column.ColumnName == "WorkingTimeOut")
                {
                    var workingDate = range[idx, 2].Value;

                    if (value == null || string.IsNullOrEmpty(value.ToString())) return DBNull.Value;

                    //var dt = DateTime.ParseExact(workingDate + " " + value.ToString(), new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                    var dt = DateTime.ParseExact(
                        workingDate + " " + value.ToString(),
                        new[] { "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm" },
                        CultureInfo.InvariantCulture
                    );

                    if (column.ColumnName == "WorkingTimeOut")
                    {
                        //var workingTimeIn = DateTime.ParseExact(workingDate + " " + range[idx, 3].Text, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                        var workingTimeIn = DateTime.ParseExact(
                            workingDate + " " + range[idx, 3].Value,
                            new[] { "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm" },
                            CultureInfo.InvariantCulture
                        );

                        if (dt < workingTimeIn)
                        {
                            dt = dt.AddDays(1);
                        }
                    }

                    return dt;
                }
                else if (column.ColumnName == "NoReg")
                {
                    var val = range[idx, 1].Text;

                    return val.Trim();
                }

                return null;
            });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Proxy time management page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewProxyTime)]
    public class ProxyTimeController : MvcControllerBase
    {
        /// <summary>
        /// Proxy time management service object
        /// </summary>
        protected ProxyTimeService ProxyTimeService { get { return ServiceProxy.GetService<ProxyTimeService>(); } }

        /// <summary>
        /// Time management main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var faskes = ProxyTimeService.Get(id);

            return PartialView("_ProxyTimeForm", faskes);
        }
    }
    #endregion
}