using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// SPKL overtime API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.SpklOvertime)]
    public class SpklOvertimeApiController : FormApiControllerBase<SpklOvertimeViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Spkl Service
        /// </summary>
        protected SpklService SpklService { get { return ServiceProxy.GetService<SpklService>(); } }
        #endregion

        /// <summary>
        /// Validate before create form
        /// </summary>
        /// <param name="formKey">Form Key</param>
        protected override void ValidateOnCreate(string formKey)
        {
            var keyDate = DateTime.Now;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var min = ConfigService.GetConfigValue("Spkl.Min", 3);
            var max = ConfigService.GetConfigValue("Spkl.Max", 6);
            var org = MdmService.GetActualOrganizationStructure(noreg, postCode);

            Assert.ThrowIf(org == null, "Organization cannot be empty");

            var totalSubordinate = MdmService.GetTotalSubordinate(keyDate, org.OrgCode, min, max);

            Assert.ThrowIf(totalSubordinate == 0, $"Cannot create SPKL, there is no subordinate with class {min}-{max}");
        }

        /// <summary>
        /// Validate after submit and create form
        /// </summary>
        /// <param name="requestDetailViewModel">Request Detail View Model</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<SpklOvertimeViewModel> requestDetailViewModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var tempId = requestDetailViewModel.Object.TempId;

            Assert.ThrowIf(!SpklService.IsValidDocument(noreg, tempId), "Cannot create, SPKL plan must be defined");
        }

        /// <summary>
        /// Validate after update form
        /// </summary>
        /// <param name="requestDetailViewModel">Request Detail View Model</param>
        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<SpklOvertimeViewModel> requestDetailViewModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var tempId = requestDetailViewModel.Object.TempId;

            Assert.ThrowIf(!SpklService.IsValidDocument(noreg, tempId), "Cannot create, SPKL plan must be defined");
        }

        /// <summary>
        /// Event during document created
        /// </summary>
        /// <param name="sender">Service Object</param>
        /// <param name="e">Document Request Detail View Model Object</param>
        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<SpklOvertimeViewModel>;

            SpklService.UpdateSpkl(documentRequestDetail.DocumentApprovalId, documentRequestDetail.Object.TempId);
        }

        /// <summary>
        /// Get SPKL request details by noreg and temporary id
        /// </summary>
        /// <param name="tempId">Temporary Id</param>
        /// <param name="noreg">No Reg.</param>
        /// <param name="request">Data Source Request Object</param>
        /// <returns>Data Source Result Object</returns>
        [HttpPost]
        [Route("gets")]
        public async Task<DataSourceResult> GetFromPosts([FromForm] Guid tempId, [FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            return await SpklService.GetSpklRequestDetails(tempId, noreg).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Update SPKL
        /// </summary>
        /// <param name="documentRequestDetail">Document Request Detail Object</param>
        /// <returns></returns>
        [HttpPost]
        [Route("update")]
        public IActionResult UpdateSpkl([FromBody] DocumentRequestDetailViewModel<SpklOvertimeViewModel> documentRequestDetail)
        {
            try
            {
                SpklService.UpdateSpkl(ServiceProxy.UserClaim.NoReg, documentRequestDetail);
                return Ok(new { message = "Update Success" }); 
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new  
                {
                    status = "warning",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("validate-overtime-duration")]
        public IActionResult ValidateSpkl([FromBody] DocumentRequestDetailViewModel<SpklOvertimeViewModel> documentRequestDetail)
        {
            SpklOvertimeViewModel vm = documentRequestDetail.Object;
            var numberOfEmployees = vm.NoRegs.Count();
            decimal totalBreakDuration = 0;
            decimal totalOvertimeDuration = 0;
            decimal overtimeDuration = 0;
            for(DateTime date = vm.OvertimeDate.Value; date <= vm.OvertimeDateOut; date = date.AddDays(1))
            {
                overtimeDuration = ServiceHelper.CalculateDuration((decimal)(date.Add(vm.OvertimeHourOut.Value) - date.Add(vm.OvertimeHourIn.Value)).TotalMinutes - vm.OvertimeTimeBreak);
                totalOvertimeDuration = totalOvertimeDuration + overtimeDuration;
                totalBreakDuration = totalBreakDuration + vm.OvertimeTimeBreak;
            }
            return new JsonResult(new { TotalOvertimeDuration = totalOvertimeDuration * numberOfEmployees });
        }

        /// <summary>
        /// Get SPKL by employee class
        /// </summary>
        /// <param name="tempId">Temporary Id</param>
        /// <param name="orgCode">Org Code</param>
        /// <param name="keyDate">Key Date</param>
        /// <param name="request">Data Source Request</param>
        /// <returns>Data Source Result Object</returns>
        [HttpPost("gets-by-class")]
        public DataSourceResult GetEmployeeByClass(
            [FromForm] Guid tempId,
            [FromForm] string orgCode,
            [FromForm] DateTime keyDate,
            [DataSourceRequest] DataSourceRequest request)
        {
            var min = ConfigService.GetConfigValue("Spkl.Min", 3);
            var max = ConfigService.GetConfigValue("Spkl.Max", 6);

            return ServiceProxy.GetTableValuedDataSourceResult<SpklRequestClassRangeStoredEntity>(request, new { tempId, orgCode, keyDate, min, max });
        }

        /// <summary>
        /// Delete spkl request by id
        /// </summary>
        /// <remarks>
        /// Delete spkl request by id
        /// </remarks>
        /// <param name="id">SPKL Request Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            SpklService.Delete(id);

            return NoContent();
        }

        /// <summary>
        /// Download SPKL plan by organization
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <returns>List of SPKL Plan in Excel Format</returns>
        [HttpGet("download")]
        public IActionResult Download(string orgCode)
        {
            var keyDate = DateTime.Now.Date;
            var min = ConfigService.GetConfigValue("Spkl.Min", 3);
            var max = ConfigService.GetConfigValue("Spkl.Max", 6);
            var pathProvider = ServiceProxy.GetPathProvider();
            var templatePath = pathProvider.ContentPath("uploads\\excel-template\\spkl-planning.xlsx");
            var data = MdmService.GetEmployeeByClassRange(orgCode, keyDate, min, max).OrderBy(x => x.Name);
            var fileName = string.Format("SPKL-PLAN-TEMPLATE-{0:ddMMyyyy}.xlsx", keyDate);

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
                        var defaultTime = "00:00";
                        var defaultBreak = 0;
                        var defaultReason = "Wajib Diisi";
                        var defaultOvertimeCategory = "Pekerjaan 5 R";

                        foreach (var item in data)
                        {
                            sheet.Cells[rowIndex, 1].Value = int.Parse(item.NoReg);
                            sheet.Cells[rowIndex, 2].Value = item.Name;
                            sheet.Cells[rowIndex, 3].Value = defaultDate;
                            sheet.Cells[rowIndex, 3].Style.QuotePrefix = true;
                            sheet.Cells[rowIndex, 4].Value = defaultTime;
                            sheet.Cells[rowIndex, 4].Style.QuotePrefix = true;
                            sheet.Cells[rowIndex, 5].Value = defaultTime;
                            sheet.Cells[rowIndex, 5].Style.QuotePrefix = true;
                            sheet.Cells[rowIndex, 6].Value = defaultBreak;
                            sheet.Cells[rowIndex, 7].Value = defaultOvertimeCategory;
                            sheet.Cells[rowIndex, 8].Value = defaultReason;

                            sheet.Cells[rowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            sheet.Cells[rowIndex, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                            rowIndex++;
                        }

                        package.SaveAs(ms);
                    }

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        /// <summary>
        /// Upload SPKL planning by document approval id and temporary id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <param name="tempId">Temporary Id</param>
        /// <returns>Upload Summary Object</returns>
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([FromForm] Guid documentApprovalId, [FromForm] Guid tempId)
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];
            var spklCategories = ConfigService.GetGeneralCategories("CategorySPKL").ToDictionary(x => x.Name);
            var now = DateTime.Now.Date;
            var canUploadBackDate = AclHelper.HasPermission("Core.CanUploadSpklBackDate");
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
                            new DataColumn("NoReg", typeof(string)),
                            new DataColumn("OvertimeDate", typeof(DateTime)),
                            new DataColumn("OvertimeBreak", typeof(int)),
                            new DataColumn("OvertimeIn", typeof(DateTime)),
                            new DataColumn("OvertimeOut", typeof(DateTime)),
                            new DataColumn("Duration", typeof(decimal)),
                            new DataColumn("Category", typeof(string)),
                            new DataColumn("Reason", typeof(string))
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            var noreg = workSheet.Cells[rowStart, 1].Value.ToString();
                            DateTime overtimeDate;

                            try
                            {
                                overtimeDate = DateTime.ParseExact(workSheet.Cells[rowStart, 3].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            if (overtimeDate < now && !canUploadBackDate)
                            {
                                messages.Add(string.Format("Row {0}: SPKL is backdate", rowStart));
                                rowStart++;
                                continue;
                            }

                            int overtimeBreak;
                            TimeSpan overtimeIn;
                            TimeSpan overtimeOut;

                            try
                            {
                                overtimeBreak = Math.Abs(int.Parse(workSheet.Cells[rowStart, 6].Value.ToString()));
                                overtimeIn = TimeSpan.Parse(workSheet.Cells[rowStart, 4].Value.ToString());
                                overtimeOut = TimeSpan.Parse(workSheet.Cells[rowStart, 5].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            var overtimeInDate = overtimeDate.Add(overtimeIn);
                            var overtimeOutDate = overtimeIn > overtimeOut ? overtimeDate.AddDays(1).Add(overtimeOut) : overtimeDate.Add(overtimeOut);

                            if (overtimeOutDate < overtimeInDate)
                            {
                                messages.Add(string.Format("Row {0}: SPKL Out is lower than SPKL In", rowStart));
                                rowStart++;
                                continue;
                            }

                            var duration = ServiceHelper.CalculateDuration((decimal)(overtimeOutDate - overtimeInDate).TotalMinutes - overtimeBreak);
                            var category = (workSheet.Cells[rowStart, 7].Value ?? string.Empty).ToString();

                            if (string.IsNullOrEmpty(category))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Category is empty", rowStart));
                                rowStart++;
                                continue;
                            }

                            if (!spklCategories.ContainsKey(category))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Category is not in list", rowStart));
                                rowStart++;
                                continue;
                            }

                            var categoryCode = spklCategories.ContainsKey(category) ? spklCategories[category].Code : string.Empty;
                            var reason = workSheet.Cells[rowStart, 8].Text;

                            if (string.IsNullOrEmpty(reason))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Reason is empty", rowStart));
                                rowStart++;
                                continue;
                            }

                            var row = dt.NewRow();
                            row.ItemArray = new object[] {
                                noreg,
                                overtimeDate,
                                overtimeBreak,
                                overtimeInDate,
                                overtimeOutDate,
                                duration,
                                categoryCode,
                                reason
                            };

                            dt.Rows.Add(row);

                            rowStart++;
                            totalSuccess++;
                        }

                        totalUpload = rowStart - initialStart;

                        SpklService.UploadSpkl(actor, postCode, documentApprovalId, tempId, dt);
                    }
                }
            }

            return Ok(new { TotalUpload = totalUpload, TotalSuccess = totalSuccess, TotalFailed = totalUpload - totalSuccess, Messages = messages });
        }

        [HttpPost]
        [Route("validate-upload")]
        public async Task<IActionResult> ValidateUpload([FromForm] decimal remainingOvertimeDuration)
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];
            var spklCategories = ConfigService.GetGeneralCategories("CategorySPKL").ToDictionary(x => x.Name);
            var now = DateTime.Now.Date;
            var canUploadBackDate = AclHelper.HasPermission("Core.CanUploadSpklBackDate");
            var totalSuccess = 0;
            var messages = new List<string>();
            decimal totalOvertimeDuration = 0;
            bool remainingOvertimeDurationExceeded = false;

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
                            new DataColumn("OvertimeDate", typeof(DateTime)),
                            new DataColumn("OvertimeBreak", typeof(int)),
                            new DataColumn("OvertimeIn", typeof(DateTime)),
                            new DataColumn("OvertimeOut", typeof(DateTime)),
                            new DataColumn("Duration", typeof(decimal)),
                            new DataColumn("Category", typeof(string)),
                            new DataColumn("Reason", typeof(string))
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            var noreg = workSheet.Cells[rowStart, 1].Value.ToString();
                            DateTime overtimeDate;

                            try
                            {
                                overtimeDate = DateTime.ParseExact(workSheet.Cells[rowStart, 3].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            if (overtimeDate < now && !canUploadBackDate)
                            {
                                messages.Add(string.Format("Row {0}: SPKL is backdate", rowStart));
                                rowStart++;
                                continue;
                            }

                            int overtimeBreak;
                            TimeSpan overtimeIn;
                            TimeSpan overtimeOut;

                            try
                            {
                                overtimeBreak = Math.Abs(int.Parse(workSheet.Cells[rowStart, 6].Value.ToString()));
                                overtimeIn = TimeSpan.Parse(workSheet.Cells[rowStart, 4].Value.ToString());
                                overtimeOut = TimeSpan.Parse(workSheet.Cells[rowStart, 5].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            var overtimeInDate = overtimeDate.Add(overtimeIn);
                            var overtimeOutDate = overtimeIn > overtimeOut ? overtimeDate.AddDays(1).Add(overtimeOut) : overtimeDate.Add(overtimeOut);

                            if (overtimeOutDate < overtimeInDate)
                            {
                                messages.Add(string.Format("Row {0}: SPKL Out is lower than SPKL In", rowStart));
                                rowStart++;
                                continue;
                            }

                            var duration = ServiceHelper.CalculateDuration((decimal)(overtimeOutDate - overtimeInDate).TotalMinutes - overtimeBreak);
                            var category = (workSheet.Cells[rowStart, 7].Value ?? string.Empty).ToString();

                            if (string.IsNullOrEmpty(category))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Category is empty", rowStart));
                                rowStart++;
                                continue;
                            }

                            if (!spklCategories.ContainsKey(category))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Category is not in list", rowStart));
                                rowStart++;
                                continue;
                            }

                            var categoryCode = spklCategories.ContainsKey(category) ? spklCategories[category].Code : string.Empty;
                            var reason = workSheet.Cells[rowStart, 8].Text;

                            if (string.IsNullOrEmpty(reason))
                            {
                                messages.Add(string.Format("Row {0}: SPKL Reason is empty", rowStart));
                                rowStart++;
                                continue;
                            }

                            var row = dt.NewRow();
                            row.ItemArray = new object[] {
                                noreg,
                                overtimeDate,
                                overtimeBreak,
                                overtimeInDate,
                                overtimeOutDate,
                                duration,
                                categoryCode,
                                reason
                            };

                            dt.Rows.Add(row);

                            rowStart++;
                            totalSuccess++;
                            totalOvertimeDuration = totalOvertimeDuration + duration;
                        }

                        if(messages.Count == 0)
                        {
                            if (totalOvertimeDuration > remainingOvertimeDuration)
                            {
                                remainingOvertimeDurationExceeded = true;
                            }
                        }
                    }
                }
            }

            return Ok(new { Messages = messages, RemainingOvertimeDurationExceeded = remainingOvertimeDurationExceeded, TotalOvertimeDuration = totalOvertimeDuration, RemainingOvertimeDuration = remainingOvertimeDuration });
        }
    }
    #endregion
}