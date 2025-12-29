using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validators;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Validators;
using TAMHR.ESS.Infrastructure.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System;
using OfficeOpenXml;
using System.Drawing;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Employee Leave API Manager
    /// </summary>
    [Route("api/employee-leave")]
    [Permission(PermissionKey.ManageEmployeeLeave)]
    public class EmployeeLeaveApiController : GenericApiControllerBase<EmployeeLeaveService, EmployeeLeave>
    {
        protected override string[] ComparerKeys => new[] { "NoReg" };
        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var data = CommonService.GetQuery().ToList();

            return await Task.FromResult(data.ToDataSourceResult(request));
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Employee leave page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewEmployeeLeave)]
    public class EmployeeLeaveController : MvcControllerBase
    {
        public EmployeeLeaveService EmployeeLeaveService => ServiceProxy.GetService<EmployeeLeaveService>();
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Load(string noreg)
        {
            // This returns a List<EmployeeLeaveHistoryView>
            var model = EmployeeLeaveService.GetEmployeeLeaveHistory(noreg);

            // Just return the model as is
            return PartialView("~/Areas/MasterData/Views/Shared/_EmployeeLeaveForm.cshtml", model);
        }

        [HttpPost]
        public IActionResult LoadAddForm(string noreg)
        {
            // This returns a List<EmployeeLeaveHistoryView>
            var model = EmployeeLeaveService.GetEmployeeLeaveHistory(noreg);

            // Just return the model as is
            return PartialView("~/Areas/MasterData/Views/Shared/_EmployeeLeaveFormAdd.cshtml", model);
        }

        [HttpPost]
        public IActionResult LoadAddLeave(string noreg)
        {

            var model = EmployeeLeaveService.GetEmployeeLeaveHistory(noreg);

            // **Set ViewBag for the view to use**
            ViewBag.SelectedNoReg = noreg;

            return PartialView("~/Areas/MasterData/Views/Shared/_EmployeeLeaveFormAdd.cshtml", model);
        }


        [HttpPost]
        public IActionResult UpdateLeave([FromBody] EmployeeLeaveViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;

            var actualUsed = EmployeeLeaveService.GetUsedAnnualLeaveWithoutManual(model.noreg, model.Period);

            if (model.UsedLeave < actualUsed)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Used leave cannot be less than already recorded leave: {actualUsed} days."
                });
            }

            bool isUpdated = EmployeeLeaveService.UpdateLeave(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Leave updated successfully." });

            return BadRequest(new { success = false, message = "Failed to update leave." });
        }

        [HttpPost]
        public IActionResult UpdateLongLeave([FromBody] EmployeeLeaveViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;

            // Ambil total cuti tanpa manual
            var actualUsed = EmployeeLeaveService.GetUsedLongLeaveWithoutManual(model.noreg, model.Period);

            // Validasi: jumlah yang diajukan tidak boleh lebih kecil dari cuti yang sudah digunakan
            if (model.UsedLeave < actualUsed)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"The number of leave days cannot be less than the number of days already used: {actualUsed} days."
                });
            }

            bool isUpdated = EmployeeLeaveService.UpdateLongLeave(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Leave updated successfully." });

            return BadRequest(new { success = false, message = "Failed to update leave." });
        }

        [HttpPost]
        public async Task<IActionResult> Merges()
        {
            var userNoReg = ServiceProxy.UserClaim.NoReg;
            var file = Request.Form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            await EmployeeLeaveService.ProcessUploadAsync(file, userNoReg);

            return NoContent();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string filePath = Path.Combine(wwwRootPath, "uploads", "excel-template", "Template_EmployeeLeave.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Template file not found." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.ms-excel", "Template_EmployeeLeave.xlsx");
        }

        [HttpPost("download-report")]
        public IActionResult Download()
        {
            var getData = EmployeeLeaveService.GetEmployeeReports().ToList();

            //if (!string.IsNullOrEmpty(noreg))
            //{
            //    var noRegList = noreg.Split(";", StringSplitOptions.RemoveEmptyEntries);
            //    getData = getData.Where(c => noRegList.Contains(c.NoReg));
            //}

            //var dataList = getData.ToList();

            string title = "Employee Profile";

            var fileName = string.Format(title + " Report.xlsx");

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Employee Leave");

                    var cols = new[] { "NoReg", "Name", "Total Annual Leave", "Used Annual Leave", "Remaining Annual Leave", "Total Long Leave", "Used Long Leave", "Remaining Long Leave", "Entry Date", "Start Date", "End Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                        sheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#b4c6e7"));

                    }
                    sheet.Column(1).Width = 20;
                    sheet.Column(2).Width = 30;
                    sheet.Column(3).Width = 5;
                    sheet.Column(4).Width = 5;
                    sheet.Column(5).Width = 5;
                    sheet.Column(6).Width = 5;
                    sheet.Column(7).Width = 5;
                    sheet.Column(8).Width = 5;
                    sheet.Column(9).Width = 20;
                    sheet.Column(10).Width = 20;
                    sheet.Column(11).Width = 20;

                    foreach (var data in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = data.noreg;
                        sheet.Cells[rowIndex, 2].Value = data.Name;
                        sheet.Cells[rowIndex, 3].Value = data.TotalAnnualLeave;
                        sheet.Cells[rowIndex, 4].Value = data.UsedAnnualLeave;
                        sheet.Cells[rowIndex, 5].Value = data.AnnualLeave;
                        sheet.Cells[rowIndex, 6].Value = data.TotalLongLeave;
                        sheet.Cells[rowIndex, 7].Value = data.UsedLongLeave;
                        sheet.Cells[rowIndex, 8].Value = data.LongLeave;
                        sheet.Cells[rowIndex, 9].Value = data.DateIn;
                        sheet.Cells[rowIndex, 9].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 10].Value = data.CreatedOn;
                        sheet.Cells[rowIndex, 10].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 11].Value = data.perioddateleave;
                        sheet.Cells[rowIndex, 11].Style.Numberformat.Format = "dd-MM-yyyy";

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }
                    package.SaveAs(ms);
                    
                    //return ExportToXlsx(data.ToList(), "EmployeeLeave");
                }
                return Ok(Convert.ToBase64String(ms.ToArray()));
            }
        }


        [HttpPost]
        public IActionResult AddLeaveEmployee([FromBody] AddEmployeeLeaveViewModel model)
        {
        
            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;
            bool isUpdated = EmployeeLeaveService.AddLeaveEmployee(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Leave details saved successfully." }); 

            return BadRequest(new { success = false, message = "Failed to save leave details." });

        }


        protected IActionResult GetViewData(object viewData)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Controller", string.Empty);
            return PartialView($"_{controllerName}Form", viewData); 
        }

    }
    #endregion
}