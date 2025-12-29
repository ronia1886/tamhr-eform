using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using OfficeOpenXml.Style;
using System.Web;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// VaccineSchedule API Manager
    /// </summary>
    [Route("api/vaccineschedule")]
    public class VaccineScheduleApiController : GenericApiControllerBase<VaccineScheduleService, VaccineSchedule>
    {
        /// <summary>
        /// Comparer for upload
        /// </summary>
        protected override string[] ComparerKeys => new[] { "VaccineNumber" };
        protected VaccineScheduleService vaccineScheduleService => ServiceProxy.GetService<VaccineScheduleService>();
        protected VaccineHospitalService hospitalService => ServiceProxy.GetService<VaccineHospitalService>();
        /// <summary>
        /// Get list of VaccineSchedules
        /// </summary>
        /// <param name="request">Data Source Request Object</param>
        /// <returns>List of VaccineSchedules</returns>
        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetQuery().OrderBy(x => x.VaccineNumber).ToDataSourceResultAsync(request);
        }

        [HttpPost("get-details")]
        public async Task<DataSourceResult> GetDetail([FromForm] Guid vaccineScheduleId, [DataSourceRequest] DataSourceRequest request)
        {
            var parameters = new
            {
                VaccineScheduleId = vaccineScheduleId,
            };
            return await Task.FromResult(ServiceProxy.GetTableValuedDataSourceResult<VaccineScheduleLimitStoredEntity>(request, parameters));
        }

        [HttpPost("upsert-detail")]
        public IActionResult CreateQuestionDetail([FromBody] VaccineScheduleLimit vaccineScheduleLimit)
        {
            var output = CommonService.UpsertDetail(vaccineScheduleLimit);

            return Ok(output);
        }

        [HttpPut("upsert-detail")]
        public IActionResult UpdateQuestionDetail([FromBody] VaccineScheduleLimit vaccineScheduleLimit)
        {
            var output = CommonService.UpsertDetail(vaccineScheduleLimit);

            return Ok(output);
        }

        [HttpDelete("delete-detail")]
        public IActionResult DeleteDetailById([FromForm] Guid id)
        {

            var output = CommonService.DeleteDetailById(id);

            return Ok(output);
        }

        [HttpGet]
        public IActionResult GetLastBatch()
        {
            int lastBatch = vaccineScheduleService.GetLastBatch();

            return Ok(lastBatch);
        }

        [HttpPost("get-detail-grids")]
        public async Task<DataSourceResult> GetDetailGrid([FromForm] Guid vaccineScheduleId, [DataSourceRequest] DataSourceRequest request)
        {
            var parameters = new
            {
                VaccineScheduleId = vaccineScheduleId,
            };
            return await Task.FromResult(ServiceProxy.GetTableValuedDataSourceResult<VaccineScheduleDetailGridStoredEntity>(request, parameters));
        }

        [HttpGet("get-detail-sum")]
        public async Task<DataSourceResult> GetDetailGridSum(Guid vaccineScheduleId, [DataSourceRequest] DataSourceRequest request)
        {
            var parameters = new
            {
                VaccineScheduleId = vaccineScheduleId,
            };
            return await Task.FromResult(ServiceProxy.GetTableValuedDataSourceResult<VaccineScheduleDetailGridStoredEntity>(request, parameters));
        }

        public override async Task<IActionResult> Merge()
        {
            var dicts = new Dictionary<string, Type>
            {
                { "VaccineScheduleId", typeof(string) },
                { "VaccineHospitalId", typeof(string) },
                { "VaccineDate", typeof(DateTime) },
                { "Qty", typeof(int) }
            };

            var foreignKeys = new Dictionary<string, string> {
                { "VaccineHospitalId", "Id|VaccineScheduleId:VaccineScheduleId,VaccineHospitalId:VaccineHospitalId|dbo.TB_M_VACCINE_SCHEDULE_LIMIT" }
            };

            var columnKeys = new[] { "VaccineDate", "Qty" };

            await UploadAndMergeAsync<VaccineScheduleLimit>(Request.Form.Files.FirstOrDefault(), dicts, columnKeys, foreignKeys);

            return NoContent();
        }

        [HttpGet("download/download-template")]
        public IActionResult DownloadVaccineSchedule(string orgCode)
        {
            var keyDate = DateTime.Now.Date;
            var pathProvider = ServiceProxy.GetPathProvider();
            var templatePath = pathProvider.ContentPath("uploads\\excel-template\\vaccineschedule-template.xlsx");
            var fileName = string.Format("VACCINE-SCHEDULE-TEMPLATE-{0:ddMMyyyy}.xlsx", keyDate);

            using (var ms = new MemoryStream())
            {
                using (var stream = System.IO.File.OpenRead(templatePath))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        var sheet = package.Workbook.Worksheets[0];
                        var now = DateTime.Now;
                        var defaultDate = now.ToString("dd/MM/yyyy");
                        //var defaultTime = "00:00";
                        //var defaultBreak = 0;
                        //var defaultReason = "Wajib Diisi";
                        //var defaultOvertimeCategory = "Pekerjaan 5 R";

                        //foreach (var item in data)
                        //{
                        //    sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
                        //    sheet.Cells[rowIndex, 2].Value = item.Name;
                        //    sheet.Cells[rowIndex, 3].Value = defaultDate;
                        //    sheet.Cells[rowIndex, 3].Style.QuotePrefix = true;
                        //    sheet.Cells[rowIndex, 4].Value = defaultTime;
                        //    sheet.Cells[rowIndex, 4].Style.QuotePrefix = true;
                        //    sheet.Cells[rowIndex, 5].Value = defaultTime;
                        //    sheet.Cells[rowIndex, 5].Style.QuotePrefix = true;
                        //    sheet.Cells[rowIndex, 6].Value = defaultBreak;
                        //    sheet.Cells[rowIndex, 7].Value = defaultOvertimeCategory;
                        //    sheet.Cells[rowIndex, 8].Value = defaultReason;

                        //    sheet.Cells[rowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        //    sheet.Cells[rowIndex, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                        //    rowIndex++;
                        //}

                        package.SaveAs(ms);
                    }

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet("download/download-data")]
        public IActionResult DownloadDataVaccineSchedule(string orgCode)
        {
            var keyDate = DateTime.Now.Date;
            var pathProvider = ServiceProxy.GetPathProvider();
            var templatePath = pathProvider.ContentPath("uploads\\excel-template\\vaccineschedule-template.xlsx");
            var fileName = string.Format("Exported-VaccineSchedule-{0:dd-MM-yyyy}.xlsx", keyDate);

            using (var ms = new MemoryStream())
            {
                using (var stream = System.IO.File.OpenRead(templatePath))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        int rowIndex = 2;
                        var sheet = package.Workbook.Worksheets[0];
                        var now = DateTime.Now;
                        var defaultDate = now.ToString("dd/MM/yyyy");
                        
                        sheet.Cells[1, 7].Value = "Remain Quota";
                        sheet.Cells[1, 4].AutoFitColumns();
                        sheet.Cells[1, 7].AutoFitColumns();

                        sheet.Cells[1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                        sheet.Cells[1, 7].Style.Font.Color.SetColor(System.Drawing.Color.White);

                        var data = vaccineScheduleService.DownloadDataVaccineSchedule();
                        foreach (var item in data)
                        {
                            sheet.Cells[rowIndex, 1].Value = item.VaccineNumber;
                            sheet.Cells[rowIndex, 2].Value = item.StartDateTime.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 3].Value = item.EndDateTime.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 4].Value = HttpUtility.HtmlEncode(item.Hospital);
                            sheet.Cells[rowIndex, 5].Value = item.VaccineDate.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 6].Value = item.Qty;
                            sheet.Cells[rowIndex, 7].Value = item.RemainingQty;
                            

                            sheet.Cells[rowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                            rowIndex++;
                        }

                        package.SaveAs(ms);
                    }

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];
            //var spklCategories = ConfigService.GetGeneralCategories("CategorySPKL").ToDictionary(x => x.Name);
            var now = DateTime.Now.Date;
            //var canUploadBackDate = AclHelper.HasPermission("Core.CanUploadSpklBackDate");
            var totalSuccess = 0;
            var totalUpload = 0;
            var messages = new List<string>();

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
                            new DataColumn("Id", typeof(Guid)),
                            new DataColumn("Batch", typeof(string)),
                            new DataColumn("StartDate", typeof(DateTime)),
                            new DataColumn("EndDate", typeof(DateTime)),
                            new DataColumn("Hospital", typeof(string)),
                            new DataColumn("HospitalDate", typeof(DateTime)),
                            new DataColumn("Quota", typeof(int)),
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            var trueDate = true;
                            int Batch;
                            try
                            {
                                Batch = Convert.ToInt32(workSheet.Cells[rowStart, 1].Value.ToString());
                            }
                            catch(Exception ex)
                            {
                                trueDate = false;
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }
                            

                            
                            DateTime StartDate;
                            try
                            {
                                StartDate = DateTime.ParseExact(workSheet.Cells[rowStart, 2].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                trueDate = false;
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            DateTime EndDate;
                            try
                            {
                                EndDate = DateTime.ParseExact(workSheet.Cells[rowStart, 3].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                trueDate = false;
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            if (trueDate == true && EndDate >= StartDate)
                            {
                                var checkBatchAndDate = vaccineScheduleService.CheckBatchAndDate(Batch,StartDate,EndDate);
                                if (!checkBatchAndDate)
                                {
                                    messages.Add(string.Format("Row {0}: {1}", rowStart, "Batch is exits but Start Date and End Date are different"));
                                    rowStart++;
                                    continue;
                                }
                                
                                
                            }
                            else
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, "Start Date can't more than End Date"));
                                rowStart++;
                                continue;
                            }

                            
                            var Hospital = workSheet.Cells[rowStart, 4].Text;

                            if (string.IsNullOrEmpty(Hospital))
                            {
                                messages.Add(string.Format("Row {0}: Hospital is empty", rowStart));
                                rowStart++;
                                continue;
                            }
                            else
                            {
                                var checkHospitalName = hospitalService.GetHospitalByName(Hospital);
                                if (checkHospitalName == null)
                                {
                                    messages.Add(string.Format("Row {0}: Hospital not exists in Vaccine Hospital", rowStart));
                                    rowStart++;
                                    continue;
                                }
                            }

                            DateTime HospitalDate;
                            try
                            {
                                HospitalDate = DateTime.ParseExact(workSheet.Cells[rowStart, 5].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                                var checkHospitalNameDate = hospitalService.GetScheduleByHospitalDate(Hospital,HospitalDate);
                                if (checkHospitalNameDate != null)
                                {
                                    messages.Add(string.Format("Row {0}: Hospital ["+Hospital+"] with Hospital Date ["+HospitalDate.ToString("d/M/yyyy")+"]  is exists", rowStart));
                                    rowStart++;
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            int Quota;

                            try
                            {
                                Quota = Math.Abs(int.Parse(workSheet.Cells[rowStart, 6].Value.ToString()));
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            var row = dt.NewRow();
                            row.ItemArray = new object[] {
                                Guid.NewGuid(),
                                Batch,
                                StartDate,
                                EndDate,
                                Hospital,
                                HospitalDate,
                                Quota
                            };

                            dt.Rows.Add(row);

                            rowStart++;
                            totalSuccess++;
                        }

                        totalUpload = rowStart - initialStart;

                        vaccineScheduleService.UploadVaccineSchedule(actor, postCode,dt);
                    }
                }
            }

            return Ok(new { TotalUpload = totalUpload, TotalSuccess = totalSuccess, TotalFailed = totalUpload - totalSuccess, Messages = messages });
        }


        [HttpPost("get-vaccine-hospital")]
        public async Task<DataSourceResult> GetVaccineHospitals([DataSourceRequest] DataSourceRequest request)
        {
            return await vaccineScheduleService.GetVaccineHospitals().ToDataSourceResultAsync(request);
        }

    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// VaccineSchedule page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewVaccineSchedule)]
    public class VaccineScheduleController : GenericMvcControllerBase<VaccineScheduleService, VaccineSchedule>
    {
        public IActionResult LoadDetail(Guid Id, Guid VaccineScheduleId)
        {
            VaccineScheduleLimit dataDetail = new VaccineScheduleLimit();
            dataDetail.VaccineScheduleId = VaccineScheduleId;
            var data = CommonService.GetVaccineScheduleLimitById(Id);
            if (data != null)
            {
                dataDetail = data;
            }

            ViewBag.VaccineSchedule = CommonService.GetVaccineSchedule(VaccineScheduleId).FirstOrDefault();
            ViewBag.VaccineHospitalList = CommonService.GetVaccineHospitals().ToList();
            return PartialView("_VaccineScheduleDetailForm", dataDetail);
        }

        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new VaccineSchedule();
            }
            else
            {
                commonData = CommonService.GetById(id);
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}