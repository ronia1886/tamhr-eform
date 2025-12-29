using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common.Utility;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using Agit.Common;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// SPKL Master Data API Manager
    /// </summary>
    [Route("api/master-data/spkl")]
    [Permission(PermissionKey.ManageSpklMasterData)]
    public class SpklMasterDataApiController : GenericApiControllerBase<SpklMasterDataService, TimeManagementSpkl>
    {
        #region Classes
            private class InformationAbnormalityOT
            {
                public string NoReg { get; set; }
                public List<DateTime> listDate { get; set; }
            }
        #endregion
        #region Domain Services
        public MdmService MdmService => ServiceProxy.GetService<MdmService>();
        public SpklMasterDataService SpklMasterDataService => ServiceProxy.GetService<SpklMasterDataService>();

        /// <summary>
        /// SPKL service object.
        /// </summary>
        protected SpklService SpklService => ServiceProxy.GetService<SpklService>();

        public AbnormalityOverTimeService AbnormalityOverTimeService => ServiceProxy.GetService<AbnormalityOverTimeService>();
        #endregion

        protected override string[] ComparerKeys => new[] { "NoReg", "OvertimeDate", "OvertimeInPlan" };

        [HttpPost("get-views")]
        public DataSourceResult GetFromView([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetDataSourceResult<SpklMasterDataView>(request);
        }

        /// <summary>
        /// Download LKL template
        /// </summary>
        /// <returns>LKL Template in Excel</returns>
        [HttpGet("single/download-template")]
        public IActionResult DownloadTemplateSingle()
        {
            return GenerateTemplate<TimeManagementSpkl>("LKL-SINGLE-TEMPLATE.xlsx");

            //var keyDate = DateTime.Now.Date;
            //var noreg = ServiceProxy.UserClaim.NoReg;
            //var postCode = ServiceProxy.UserClaim.PostCode;
            //var username = ServiceProxy.UserClaim.Username;
            //var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            //var orgCode = organizationStructure?.OrgCode;

            ////var data = SpklService.GetSpklRequestDetailsByUser(noreg, username, orgCode).Where(x => x.EnableDocumentAction && x.DocumentStatusCode == "inprogress").OrderBy(x => x.Name).ThenBy(x => x.OvertimeDate);
            //var fileName = string.Format("LKL-SINGLE-TEMPLATE.xlsx");

            //using (var ms = new MemoryStream())
            //{
            //    using (var package = new ExcelPackage())
            //    {
            //        int rowIndex = 2;
            //        var sheet = package.Workbook.Worksheets.Add("Output");
            //        var format = "dd/MM/yyyy";

            //        var cols = new[] { "Id", "Noreg", "Name", "Overtime Date", "Overtime In Adjust", "Overtime Out Adjust", "Overtime Break Adjust", "Overtime Category", "Overtime Reason" };

            //        sheet.Column(1).Hidden = true;

            //        for (var i = 1; i <= cols.Length; i++)
            //        {
            //            sheet.Cells[1, i].Value = cols[i - 1];
            //            sheet.Cells[1, i].Style.Font.Bold = true;
            //            sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //        }

            //        foreach (var item in data)
            //        {
            //            sheet.Cells[rowIndex, 1].Value = item.Id;
            //            sheet.Cells[rowIndex, 2].Value = int.Parse(item.NoReg);
            //            sheet.Cells[rowIndex, 3].Value = item.Name;
            //            sheet.Cells[rowIndex, 4].Value = item.OvertimeDate.ToString(format);
            //            sheet.Cells[rowIndex, 4].Style.QuotePrefix = true;
            //            sheet.Cells[rowIndex, 5].Value = item.OvertimeInAdjust.HasValue ? item.OvertimeInAdjust.Value.ToString("HH:mm") : "";
            //            sheet.Cells[rowIndex, 5].Style.QuotePrefix = true;
            //            sheet.Cells[rowIndex, 6].Value = item.OvertimeOutAdjust.HasValue ? item.OvertimeOutAdjust.Value.ToString("HH:mm") : "";
            //            sheet.Cells[rowIndex, 6].Style.QuotePrefix = true;
            //            sheet.Cells[rowIndex, 7].Value = item.OvertimeBreakAdjust ?? 0;
            //            sheet.Cells[rowIndex, 8].Value = item.OvertimeCategory;
            //            sheet.Cells[rowIndex, 9].Value = item.OvertimeReason;

            //            for (var i = 1; i <= cols.Length; i++)
            //            {
            //                sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //            }

            //            rowIndex++;
            //        }

            //        package.SaveAs(ms);
            //    }

            //    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            //}
        }

        /// Download data
        /// </summary>
        /// <remarks>
        /// Download data
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download-data")]
        public IActionResult DownloadData(string noreg, string name, DateTime? sd, DateTime? ed)
        {
            var data = CommonService.GetDataViews(noreg, name, sd, ed);

            return ExportToXlsx(data, $"LKL", excludes: new[] { "Id", "OvertimeCategoryCode", "ShiftCode" }, callback: (column, value, range) =>
            {
                if (ObjectHelper.IsIn(column, new[] { "WorkingTimeIn", "WorkingTimeOut", "OvertimeInPlan", "OvertimeOutPlan", "OvertimeInAdjust", "OvertimeOutAdjust", "OvertimeDate" }))
                {
                    if (column != "OvertimeDate")
                    {
                        range.Value = value == null
                            ? string.Empty
                            : (value is DateTime ? ((DateTime)value).ToString("HH:mm") : value.ToString());
                    }

                    range.Style.QuotePrefix = value != null;
                }
            });
        }

        [HttpGet("multiple/download-template")]
        public IActionResult DownloadMultipleTemplate(DateTime? keyDate)
        {
            using (var package = new ExcelPackage())
            {
                var date = keyDate ?? DateTime.Now;
                var totalDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var culture = new CultureInfo(ApplicationConstants.IndoCultureInfo);
                var name = "LKL-MULTIPLE-TEMPLATE.xlsx";
                var worksheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(name));
                var month = date.ToString("MMM yyyy", culture);
                //var data = MdmService.GetEmployeeByClassRange("%", date, 3, 6).OrderBy(x => x.Name);
                var overtimeData = SpklMasterDataService.GetTimeEvaluationOvertime(date.Year, date.Month).ToList();
                var data = overtimeData.Select(x=> new
                {
                    Name = x.Name,
                    NoReg = x.NoReg
                }).Distinct().ToList();
                var dictOvertime = overtimeData.ToDictionary(x => x.NoReg + "_" + x.Day);
                var rowStart = 3;

                worksheet.Cells[1, 1, 2, 1].Merge = true;
                worksheet.Cells[1, 2, 2, 2].Merge = true;
                worksheet.Cells[1, 3, 1, totalDayOfMonth + rowStart - 1].Merge = true;

                var noregCell = worksheet.Cells[1, 1];
                noregCell.Value = "No Reg.";
                noregCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                noregCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                noregCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                var nameCell = worksheet.Cells[1, 2];
                nameCell.Value = "Name";
                nameCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                nameCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                nameCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                var monthCell = worksheet.Cells[1, 3];
                monthCell.Value = month;
                monthCell.Style.QuotePrefix = true;
                monthCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                monthCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                for (var i = 3; i < totalDayOfMonth + 3; i++)
                {
                    worksheet.Cells[2, i].Value = $"Day {i - 3 + 1}";
                    worksheet.Cells[2, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                foreach (var emp in data)
                {
                    worksheet.Cells[rowStart, 1].Value = emp.NoReg;
                    worksheet.Cells[rowStart, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowStart, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheet.Cells[rowStart, 2].Value = emp.Name;
                    worksheet.Cells[rowStart, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    for (var i = 3; i < totalDayOfMonth + 3; i++)
                    {
                        var key = emp.NoReg + "_" + (i - 2);

                        if (dictOvertime.ContainsKey(key))
                        {
                            worksheet.Cells[rowStart, i].Value = dictOvertime[key].DurationAdjust;
                        }

                        worksheet.Cells[rowStart, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStart, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    rowStart++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    var fileName = $"{name}";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    memoryStream.Position = 0;

                    return File(memoryStream.ToArray(), contentType, fileName);
                }
            }
        }

        [HttpPost("single/merge")]
        public async Task<IActionResult> MergeSingle()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var columns = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "OvertimeDate", typeof(DateTime) },
                { "OvertimeInPlan", typeof(DateTime) },
                { "OvertimeOutPlan", typeof(DateTime) },
                { "OvertimeBreakPlan", typeof(int) },
                { "OvertimeInAdjust", typeof(DateTime) },
                { "OvertimeOutAdjust", typeof(DateTime) },
                { "OvertimeBreakAdjust", typeof(int) },
                { "DurationPlan", typeof(decimal) },
                { "DurationAdjust", typeof(decimal) },
                { "OvertimeCategoryCode", typeof(string) },
                { "OvertimeReason", typeof(string) }
            };

            await UploadAndMergeAsync<TimeManagementSpkl>(Request.Form.Files.FirstOrDefault(), columns, ComparerKeys, callback: (command, tableName) =>
            {
                command.CommandText = string.Format(@"
                    MERGE INTO {0} AS TARGET
                    USING (
                        SELECT
                            it.*, t.NormalTimeIn, t.NormalTimeOut, t.WorkingTimeIn, t.WorkingTimeOut, t.ShiftCode
                        FROM {0} it
                        LEFT JOIN dbo.TB_R_TIME_MANAGEMENT t ON t.NoReg = it.NoReg AND CAST(t.WorkingDate AS DATE) = it.OvertimeDate
                        WHERE t.Id IS NOT NULL
                    ) AS SOURCE ON SOURCE.NoReg = TARGET.NoReg AND SOURCE.OvertimeDate = TARGET.OvertimeDate AND SOURCE.OvertimeInPlan = TARGET.OvertimeInPlan
                    WHEN MATCHED THEN UPDATE SET
                        TARGET.DurationPlan = dbo.SC_CALCULATE_PROXY_DURATION(SOURCE.OvertimeInPlan, SOURCE.OvertimeOutPlan, SOURCE.OvertimeInPlan, SOURCE.OvertimeOutPlan, 
                        (CASE
			                WHEN SOURCE.ShiftCode not in ('1NJ8','1NS8') THEN SOURCE.NormalTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn <= SOURCE.NormalTimeIn   THEN SOURCE.NormalTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn between SOURCE.NormalTimeIn and Dateadd(minute,60,SOURCE.NormalTimein)   THEN SOURCE.WorkingTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn > Dateadd(minute,60,SOURCE.NormalTimein)   THEN Dateadd(minute,60,SOURCE.NormalTimein)
			                else SOURCE.NormalTimeIn
			            END ), 
                        (CASE
			                WHEN SOURCE.ShiftCode not in ('1NJ8','1NS8') THEN SOURCE.NormalTimeOut
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn <= SOURCE.NormalTimeIn   THEN SOURCE.NormalTimeOut
                            WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn > Dateadd(minute,60,SOURCE.NormalTimein)   THEN Dateadd(minute,60,SOURCE.NormaltimeOut)
			                WHEN SOURCE.ShiftCode in ('1NS8') and SOURCE.WorkingTimeIn > SOURCE.NormalTimeIn   THEN DATEADD(Minute,525,SOURCE.WorkingTimeIn)
			                WHEN SOURCE.ShiftCode in ('1NJ8') and SOURCE.WorkingTimeIn > SOURCE.NormalTimeIn   THEN DATEADD(Minute,540,SOURCE.WorkingTimeIn)
			                else SOURCE.NormalTimeOut
			            END ), 
                        SOURCE.OvertimeBreakPlan),
                        TARGET.DurationAdjust = dbo.SC_CALCULATE_PROXY_DURATION(SOURCE.OvertimeInPlan, SOURCE.OvertimeOutPlan, SOURCE.OvertimeInAdjust, SOURCE.OvertimeOutAdjust, 
                        (CASE
			                WHEN SOURCE.ShiftCode not in ('1NJ8','1NS8') THEN SOURCE.NormalTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn <= SOURCE.NormalTimeIn   THEN SOURCE.NormalTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn between SOURCE.NormalTimeIn and Dateadd(minute,60,SOURCE.NormalTimein)   THEN SOURCE.WorkingTimeIn
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn > Dateadd(minute,60,SOURCE.NormalTimein)   THEN Dateadd(minute,60,SOURCE.NormalTimein)
			                else SOURCE.NormalTimeIn
			            END ), 
                        (CASE
			                WHEN SOURCE.ShiftCode not in ('1NJ8','1NS8') THEN SOURCE.NormalTimeOut
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn <= SOURCE.NormalTimeIn   THEN SOURCE.NormalTimeOut
			                WHEN SOURCE.ShiftCode in ('1NJ8','1NS8') and SOURCE.WorkingTimeIn > Dateadd(minute,60,SOURCE.NormalTimein)   THEN Dateadd(minute,60,SOURCE.NormaltimeOut)
                            WHEN SOURCE.ShiftCode in ('1NS8') and SOURCE.WorkingTimeIn > SOURCE.NormalTimeIn   THEN DATEADD(Minute,525,SOURCE.WorkingTimeIn)
			                WHEN SOURCE.ShiftCode in ('1NJ8') and SOURCE.WorkingTimeIn > SOURCE.NormalTimeIn   THEN DATEADD(Minute,540,SOURCE.WorkingTimeIn)
			                else SOURCE.NormalTimeOut
			            END ), 
                        SOURCE.OvertimeBreakAdjust);
                ", tableName, noreg);
                command.ExecuteNonQuery();
            },
            valueCallback: (column, range, idx, value) =>
            {
                var workingDate = range[idx, 2].Text;
                //var dt = DateTime.ParseExact(workingDate + " " + value.ToString(), new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                var val = range[idx, 1].Text;

                var dateOnly = DateTime.ParseExact(
                workingDate,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture
            );

                if (AbnormalityOverTimeService.GetQuery().Where(x => x.NoReg == val).Select(x => x.OvertimeDate).ToList().Contains(dateOnly.Date))
                {
                    throw new Exception(
                    $"No Reg {val} already submit abnormality document at {dateOnly:dd/MM/yyyy}"
                );
                }

                if (column.ColumnName == "OvertimeInPlan" || column.ColumnName == "OvertimeOutPlan")
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString())) return DBNull.Value;

                    if (!DateTime.TryParse(value.ToString(), out var timePart))
                        throw new Exception($"Invalid time format for {column.ColumnName} at row {idx}");

                    var dt = dateOnly.Date.Add(timePart.TimeOfDay);

                    if (column.ColumnName == "OvertimeOutPlan")
                    {
                        if (DateTime.TryParse(range[idx, 3].Text, out var outTime))
                        {
                            var workingTimeIn = dateOnly.Date.Add(outTime.TimeOfDay);
                            if (dt < workingTimeIn)
                            {
                                dt = dt.AddDays(1); // kalau lewat tengah malam
                            }
                        }
                    }

                    return dt;
                }

                if (column.ColumnName == "OvertimeInAdjust" || column.ColumnName == "OvertimeOutAdjust")
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString())) return DBNull.Value;

                    if (!DateTime.TryParse(value.ToString(), out var timePart))
                        throw new Exception($"Invalid time format for {column.ColumnName} at row {idx}");

                    var dt = dateOnly.Date.Add(timePart.TimeOfDay);

                    if (column.ColumnName == "OvertimeOutAdjust")
                    {
                        if (DateTime.TryParse(range[idx, 6].Text, out var outTime))
                        {
                            var workingTimeIn = dateOnly.Date.Add(outTime.TimeOfDay);
                            if (dt < workingTimeIn)
                            {
                                dt = dt.AddDays(1);
                            }
                        }
                    }

                    return dt;
                }

                if (column.ColumnName == "NoReg")
                {
                    return val.Trim();
                }

                return null;
            });

            return NoContent();
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("multiple/merge")]
        public async Task<IActionResult> MergeMultiple()
        {
            var file = Request.Form.Files.FirstOrDefault();

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var culture = new CultureInfo("id-ID");
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    var rowStart = 3;
                    var index = 3;
                    var keyDate = DateTime.ParseExact(workSheet.Cells[1, 3].Text, "MMM yyyy", culture);
                    var daysInMonth = DateTime.DaysInMonth(keyDate.Year, keyDate.Month);

                    var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
                    var dt = new DataTable(dataTableName);

                    dt.Columns.AddRange(new DataColumn[]
                    {
                        new DataColumn("NoReg", typeof(string)),
                        new DataColumn("OvertimeDate", typeof(DateTime)),
                        new DataColumn("OvertimeDuration", typeof(decimal))
                    });

                    List<InformationAbnormalityOT> abnormalityOTList = new List<InformationAbnormalityOT>();

                    do
                    {
                        var key = workSheet.Cells[rowStart, 1].Text;

                        if (abnormalityOTList.Where(x => x.NoReg == key).Count() == 0)
                        {
                            InformationAbnormalityOT tempInfo = new InformationAbnormalityOT();

                            tempInfo.NoReg = key;

                            tempInfo.listDate = AbnormalityOverTimeService.GetQuery().Where(x => x.NoReg == key && x.AbnormalityOverTimeId != null).Select(x => x.OvertimeDate).ToList();

                            abnormalityOTList.Add(tempInfo);
                        }

                        if (string.IsNullOrEmpty(key))
                        {
                            break;
                        }

                        for (var i = 1; i <= daysInMonth; i++)
                        {
                            var date = new DateTime(keyDate.Year, keyDate.Month, i);

                            var overtimeAdjustText = workSheet.Cells[rowStart, index + i - 1].Text.Trim();

                            if (string.IsNullOrEmpty(overtimeAdjustText)) continue;

                            var overtimeAdjust = decimal.Parse(workSheet.Cells[rowStart, index + i - 1].Text);

                            if (overtimeAdjust < 0) continue;

                            List<DateTime> checkDateList = abnormalityOTList.Where(x => x.NoReg == key).Select(x => x.listDate).FirstOrDefault();

                            if (checkDateList != null)
                            {
                                if (checkDateList.Count > 0 && checkDateList.Contains(date))
                                {
                                    throw new Exception("No Reg " + key + " already submit abnormality document at " + date.ToString("dd/MM/yyyy"));
                                }
                            }

                            var arrayList = new ArrayList
                            {
                                key,
                                date,
                                overtimeAdjust
                            };

                            var row = dt.NewRow();
                            row.ItemArray = arrayList.ToArray();
                            dt.Rows.Add(row);
                        }

                        rowStart++;
                    }
                    while (true);

                    CommonService.Merge(ServiceProxy.UserClaim.NoReg, dt);
                }
            }

            return NoContent();
        }

        [HttpPost("get-unavailable-date-noreg")]
        public string GetUnavailableDate()
        {
            var noReg = this.Request.Form["NoReg"].ToString();

            List<DateTime> unavailableDate = AbnormalityOverTimeService.GetQuery().Where(x => x.NoReg == noReg && x.AbnormalityOverTimeId != null && x.Status != "Locked").Select(x => x.OvertimeDate).ToList();

            return JsonConvert.SerializeObject(unavailableDate);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// SPKL master data page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewSpklMasterData)]
    public class SpklMasterDataController : GenericMvcControllerBase<SpklMasterDataService, TimeManagementSpkl>
    {
        public override IActionResult Load(Guid id)
        {
            var data = CommonService.GetViewById(id);

            return GetViewData(data);
        }
    }
    #endregion
}