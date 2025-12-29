using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.WebUI.Extensions;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Shift planning API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ShiftPlanning)]
    public class ShiftPlanningApiController : FormApiControllerBase<ShiftPlanningViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Time management service object.
        /// </summary>
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            TimeManagementService.PreValidateShiftPlanning(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name, formKey, ServiceProxy.UserClaim.PostCode);
        }

        private string GetNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "0";
            }

            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        [HttpPost("shift-combo")]
        public IActionResult ShiftCombo()
        {
            return Ok(new { Data = Enumerable.Range(1, 4).Select(x => new { Value = x }), Total = 4 });
        }

        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> ImportShift()
        {
            var pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            var username = ServiceProxy.UserClaim.Username;
            var fileRequest = Request.Form.Files[0];
            var safeFileName = Path.GetFileName(fileRequest.FileName);
            var period = DateTime.Parse(Request.Form["Period"].ToString());
            var rootFolder = ConfigService.GetConfigs().Where(x => x.ConfigKey == "Upload.Path").FirstOrDefault().ConfigValue;
            var filesPath = !string.IsNullOrEmpty(rootFolder) ? rootFolder : pathProvider.ContentPath("uploads");

            var sb = new StringBuilder();

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            var dates = period.GetDates();

            if (fileRequest.Length > 0)
            {
                var sFileExtension = Path.GetExtension(fileRequest.FileName).ToLower();
                var fullPath = Path.Combine(rootFolder, safeFileName);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileRequest.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var workSheet = package.Workbook.Worksheets[0];
                        var totalRows = workSheet.Dimension.Rows;
                        var totalColumn = dates.Count;

                        var shifts = new List<ShiftPlanningRequestViewModel>();
                        var errorMessages = string.Empty;
                        var listshifts = ConfigService.GetGeneralCategories("ShiftCode");
                        var orgRequester = MdmService.GetActualOrganizationStructure(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode);
                        var shiftMappingRequester = ConfigService.GetGeneralCategoryMapping(orgRequester.OrgCode);

                        for (int i = 2; i <= totalRows; i++)
                        {
                            var noReg = workSheet.Cells[i, 2].Value;
                            var name = workSheet.Cells[i, 3].Value;

                            int n;
                            var isNumeric = int.TryParse(noReg?.ToString(), out n);

                            if ((noReg != null && isNumeric))
                            {
                                var orgEmp = MdmService.GetActualOrganizationStructure(noReg.ToString());
                                var orgcode = orgEmp?.OrgCode;

                                if (orgEmp != null)
                                {
                                    if ((orgEmp.Structure.Contains("(" + orgRequester.OrgCode + ")") || orgEmp.OrgCode == orgRequester.OrgCode))
                                    {
                                        for (int c = 0; c < totalColumn; c++)
                                        {
                                            var date = GetNumbers(workSheet.Cells[1, c + 4].Value?.ToString());

                                            if (int.Parse(date) == dates[c].Day)
                                            {
                                                var d = new DateTime(dates[c].Year, dates[c].Month, int.Parse(date));

                                                var code = workSheet.Cells[i, c + 4].Value?.ToString();

                                                if (string.IsNullOrEmpty(code)) continue;

                                                var dataShift = shiftMappingRequester.FirstOrDefault(x => x.Code.ToLower().EndsWith(code.ToLower()));

                                                if (dataShift == null)
                                                {
                                                    errorMessages += "Baris ke : " + (i - 1).ToString() + ", kolom ke: " + (c + 4) + ", Kode shift tidak terdaftar.<br />";

                                                    continue;
                                                }

                                                if (!string.IsNullOrEmpty(noReg.ToString()))
                                                {
                                                    var shift = shifts.FirstOrDefault(x => x.date == d && x.noreg == noReg.ToString()) ?? new ShiftPlanningRequestViewModel();

                                                    shift.date = d;
                                                    shift.noreg = noReg.ToString();
                                                    shift.name = orgEmp.Name;
                                                    shift.shift = dataShift.Name + " - " + dataShift.Description;
                                                    shift.shiftCode = dataShift.Code;

                                                    shifts.Add(shift);
                                                }
                                                else
                                                {
                                                    errorMessages += "Baris ke: " + (i - 1).ToString() + ", kolom ke: " + (c + 4) + ", Noreg tidak boleh kosong.<br />";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errorMessages += "Baris ke: " + (i - 1).ToString() + ", Noreg tidak terdaftar dalam organisasi.<br />";
                                    }
                                }
                                else
                                {
                                    errorMessages += "Baris ke: " + (i - 1).ToString() + ", Format Noreg Salah.<br />";
                                }
                            }
                        }

                        stream.Dispose();
                        System.IO.File.Delete(fullPath);

                        return Ok(new { datarequest = shifts, MsgError = errorMessages });
                    }
                };
            }

            throw new Exception("Bad Request");
        }
    }
    #endregion
}