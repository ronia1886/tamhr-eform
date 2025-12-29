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
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Employee Work Schedule API Manager
    /// </summary>
    [Route("api/master-data/employee-work-schedule")]
    [Permission(PermissionKey.ManageEmployeeWorkSchedule)]
    public class EmployeeWorkScheduleApiController : GenericApiControllerBase<EmployeeWorkScheduleService, EmployeeWorkSchedule>
    {
        protected override string[] ComparerKeys => new[] { "NoReg", "StartDate" };

        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetMasterView().ToDataSourceResultAsync(request);
        }

        public override async Task<IActionResult> Merge()
        {
            await base.Merge();

            CommonService.NormalizeEmployeeWorkSchedule();

            return NoContent();
        }

        [HttpPost("get-substitutions")]
        public async Task<DataSourceResult> GetSubstitution([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetSubstitutions().ToDataSourceResultAsync(request);
        }

        [HttpPost("upsert-substitution")]
        public IActionResult CreateSubstitution([FromBody]EmpWorkSchSubtitute input)
        {
            CommonService.UpsertSubstitution(input);

            return CreatedAtAction("Get", new { id = input.Id });
        }

        [HttpPut("upsert-substitution")]
        public IActionResult UpdateSubstitution([FromBody]EmpWorkSchSubtitute input)
        {
            CommonService.UpsertSubstitution(input);

            return NoContent();
        }

        [HttpDelete("remove-substitution")]
        public IActionResult RemoveSubstitution([FromForm]Guid id)
        {
            CommonService.RemoveSubstitution(id);

            return NoContent();
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("substitution/download-template")]
        public IActionResult DownloadSubstitutionTemplate(DateTime? keyDate)
        {
            using (var package = new ExcelPackage())
            {
                var now = keyDate.HasValue ? keyDate.Value : DateTime.Now;
                var totalDayOfMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var culture = new CultureInfo("id-ID");
                var name = "SUBSTITUTION-TEMPLATE.xlsx";
                var worksheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(name));
                var data = CommonService.GetEmployeeShift(now.Year, now.Month);
                var employee = data.Select(x => new { x.NoReg, x.Name }).Distinct().OrderBy(x => x.Name);
                var dicts = data.ToDictionary(x => x.NoReg + "_" + x.Date.Day, x => x.ShiftCode);
                var month = now.ToString("MMM yyyy", culture);
                var rowStart = 3;
                var key = string.Empty;

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

                foreach (var emp in employee)
                {
                    worksheet.Cells[rowStart, 1].Value = emp.NoReg;
                    worksheet.Cells[rowStart, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowStart, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheet.Cells[rowStart, 2].Value = emp.Name;
                    worksheet.Cells[rowStart, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    for (var i = 3; i < totalDayOfMonth + 3; i++)
                    {
                        key = emp.NoReg + "_" + (i - 2);

                        worksheet.Cells[rowStart, i].Value = dicts.ContainsKey(key) ? dicts[key] : string.Empty;
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

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("substitution/merge")]
        public IActionResult MergeSubstitution()
        {
            var file = Request.Form.Files.FirstOrDefault();

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);

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
                        new DataColumn("Date", typeof(DateTime)),
                        new DataColumn("ShiftCode", typeof(string))
                    });

                    do
                    {
                        var key = workSheet.Cells[rowStart, 1].Text;

                        if (string.IsNullOrEmpty(key))
                        {
                            break;
                        }

                        for (var i = 1; i <= daysInMonth; i++)
                        {
                            var date = new DateTime(keyDate.Year, keyDate.Month, i);
                            var shiftCode = workSheet.Cells[rowStart, index + i - 1].Text;

                            if (!string.IsNullOrEmpty(shiftCode))
                            {
                                var arrayList = new ArrayList
                                {
                                    key,
                                    date,
                                    shiftCode
                                };

                                var row = dt.NewRow();
                                row.ItemArray = arrayList.ToArray();
                                dt.Rows.Add(row);
                            }
                        }

                        rowStart++;
                    }
                    while (true);

                    CommonService.MergeSubstitution(ServiceProxy.UserClaim.NoReg, dt);
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("substitution/single/download-template")]
        public IActionResult DownloadSubstitutionTemplateSingle()
        {
            return GenerateTemplate<EmpWorkSchSubtitute>("Substitution", new[] { "Id", "ShiftCode", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" });
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("substitution/single/merge")]
        public async Task<IActionResult> MergeSubstitutionSingle()
        {
            var keys = new[] { "NoReg", "date" };
            var columns = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "Date", typeof(DateTime) },
                { "ShiftCodeUpdate", typeof(string) }
            };

            await UploadAndMergeAsync<EmpWorkSchSubtitute>(Request.Form.Files.FirstOrDefault(), columns, keys, valueCallback: (column, range, idx, value) =>
            {
                if (column.ColumnName == "NoReg")
                {
                    var noreg = range[idx, 1].Text;

                    return noreg.Trim();
                }

                return null;
            });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Employee work schedule page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewEmployeeWorkSchedule)]
    public class EmployeeWorkScheduleController : GenericMvcControllerBase<EmployeeWorkScheduleService, EmployeeWorkSchedule>
    {
        public EmployeeWorkPlanService employeeWorkPlanService => ServiceProxy.GetService<EmployeeWorkPlanService>();
        /// <summary>
        /// Load substitution data form by id
        /// </summary>
        /// <param name="id">Data Form Id</param>
        /// <returns>Data Form</returns>
        [HttpPost]
        public virtual IActionResult LoadSubstitution(Guid id)
        {
            var commonData = CommonService.GetSubstitution(id);

            return PartialView("_SubstitutionForm", commonData);
        }
        /// <summary>
        /// Load substitution data form by id
        /// </summary>
        /// <param name="id">Data Form Id</param>
        /// <returns>Data Form</returns>
        [HttpPost]
        public virtual IActionResult LoadPlan(Guid id)
        {
            var commonData = employeeWorkPlanService.GetMasterPlanById(id);

            return PartialView("_EmployeeWorkPlanForm", commonData);
        }
    }
    #endregion
}