using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using System.Collections.Generic;
using System;
using Kendo.Mvc.UI;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using System.IO;
using OfficeOpenXml;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Agit.Common;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using TAMHR.ESS.Infrastructure.Helpers;
using System.Globalization;
using TAMHR.ESS.Infrastructure;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Employee Leave API Managerapi/master-data/bdjk
    /// </summary>
    [Route("api/master-data/bdjk")]
    [Permission(PermissionKey.ManageBdjkMasterData)]
    public class BdjkController : GenericApiControllerBase<AbnormalityBdjkService, AbnormalityBdjk>
    {
        private class InformationMasterBDJK
        {
            public string NoReg { get; set; }
            public List<DateTime> listDate { get; set; }
        }

        public MdmService MdmService => ServiceProxy.GetService<MdmService>();

        protected AbnormalityBdjkService abnormalityBdjkService { get { return ServiceProxy.GetService<AbnormalityBdjkService>(); } }

        protected TimeEvaluationService timeEvaluationService { get { return ServiceProxy.GetService<TimeEvaluationService>(); } }

        protected override string[] ComparerKeys => new[] { "NoReg", "WorkingDate" };

        protected string[] SpecialExcludes = new[] { "Taxi", "UangMakanDinas", "BdjkDuration" };

        [HttpGet("download-template")]
        public override IActionResult DownloadTemplate()
        {
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\master-data-bdjk.xlsx");

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    package.SaveAs(output);
                }
            }

            var fileName = "BDJK-TEMPLATE.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(output.ToArray(), contentType, fileName);

            //string[] excludes = { "TimeManagementBdjkId", "DocumentApprovalId", "Status", "Id", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" };
            //return GenerateTemplate<AbnormalityBdjk>("BDJK-TEMPLATE.xlsx", excludes.Concat(SpecialExcludes).ToArray(), true);
        }
        public override async Task<IActionResult> Merge()
        {
            var dicts = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "BdjkCode", typeof(string) },
                { "BdjkReason", typeof(string) },
                { "ActivityCode", typeof(string) },
                { "WorkingDate", typeof(DateTime) },
                { "WorkingTimeIn", typeof(DateTime) },
                { "WorkingTimeOut", typeof(DateTime) },
                { "Taxi", typeof(bool) },
                { "UangMakanDinas", typeof(bool) },
                { "BdjkDuration", typeof(decimal) }
            };

            Console.WriteLine(string.Join(", ", dicts.Keys));

            string[] excludes = { "Name" };

            IFormFile file = Request.Form.Files.FirstOrDefault();

            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) Assert.ThrowIf(file == null, "Cannot read uploaded file as excel");
                    for (int col = 1; col <= 10; col++)
                    {
                        Console.WriteLine($"Header Col {col}: '{workSheet.Cells[1, col].Text}'");
                    }
                    int counter = 2;

                    List<InformationMasterBDJK> masterBDJKList = new List<InformationMasterBDJK>();

                    do
                    {
                        var bdjkCode = workSheet.Cells[counter, 2].Text;

                        string noReg = workSheet.Cells[counter, 1].Text.Trim();

                        if (masterBDJKList.Where(x => x.NoReg == noReg).Count() == 0)
                        {
                            InformationMasterBDJK tempInfo = new InformationMasterBDJK();

                            tempInfo.NoReg = noReg;

                            tempInfo.listDate = abnormalityBdjkService.GetQuery().Where(x => x.NoReg == noReg).Select(x => x.WorkingDate).ToList();

                            masterBDJKList.Add(tempInfo);
                        }

                        if (string.IsNullOrEmpty(bdjkCode))
                        {
                            break;
                        }

                        var culture = new System.Globalization.CultureInfo("id-ID");

                        string datePart = workSheet.Cells[counter, 5].Text;
                        string timeInPart = workSheet.Cells[counter, 6].Text;
                        string timeOutPart = workSheet.Cells[counter, 7].Text;

                        string[] dateFormats = { "dd/MM/yyyy", "d/M/yyyy" };
                        string[] timeFormats = { "H:mm", "HH:mm", "H:mm:ss", "HH:mm:ss" };

                        DateTime dtWorkingDate;
                            
                        if(DateTime.TryParseExact(datePart, dateFormats, culture, System.Globalization.DateTimeStyles.None, out dtWorkingDate))
                        {
                            List<DateTime> checkDateList = masterBDJKList.Where(x => x.NoReg == noReg).Select(x => x.listDate).FirstOrDefault();

                            if (checkDateList != null)
                            {
                                Assert.ThrowIf(checkDateList.Contains(dtWorkingDate), "Line " + counter + ": cannot insert. No Reg already submit abnormality document");
                            }

                            DateTime parsedIn, parsedOut;
                            bool validIn = DateTime.TryParseExact(timeInPart, timeFormats, culture,
                                System.Globalization.DateTimeStyles.None, out parsedIn);
                            bool validOut = DateTime.TryParseExact(timeOutPart, timeFormats, culture,
                                System.Globalization.DateTimeStyles.None, out parsedOut);

                            if (!validIn || !validOut)
                                throw new Exception($"Line {counter}: cannot parse WorkingTimeIn/Out. Check Excel format (use HH:mm).");

                            DateTime dtWorkingTimeIn = dtWorkingDate.Date.Add(parsedIn.TimeOfDay);
                            DateTime dtWorkingTimeOut = dtWorkingDate.Date.Add(parsedOut.TimeOfDay);


                            workSheet.Cells[counter, 5].Value = dtWorkingDate;
                            workSheet.Cells[counter, 6].Value = dtWorkingTimeIn;
                            workSheet.Cells[counter, 7].Value = dtWorkingTimeOut;

                            //DateTime dtWorkingTimeIn;

                            //DateTime.TryParseExact(workSheet.Cells[counter, 5].Text + " " + workSheet.Cells[counter, 6].Text, "dd/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtWorkingTimeIn);

                            //DateTime dtWorkingTimeOut;

                            //DateTime.TryParseExact(workSheet.Cells[counter, 5].Text + " " + workSheet.Cells[counter, 7].Text, "dd/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtWorkingTimeOut);

                            string[] bdjkcodelist = bdjkCode.Trim().Split(' ');

                            List<string> cleanbdjkcodelist = bdjkcodelist.Where(x => x != "D" && x != "T").ToList();

                            Assert.ThrowIf(cleanbdjkcodelist.Count > 1, "Line " + counter + ": cannot insert. Only allowed 1 mandatory BDJK Code");

                            bool Taxi = bdjkcodelist.Contains("T");
                            bool UangMakanDinas = bdjkcodelist.Contains("D");
                            workSheet.Cells[counter, 2].Value = String.Join(" ", cleanbdjkcodelist);
                            workSheet.Cells[counter, 8].Value = Taxi;
                            workSheet.Cells[counter, 9].Value = UangMakanDinas;

                            var normalSchedule = abnormalityBdjkService.GetNormalWorkSchedule(ServiceProxy.UserClaim.NoReg, dtWorkingDate);

                            decimal Duration = 0;

                            if (dtWorkingDate.DayOfWeek == DayOfWeek.Saturday || dtWorkingDate.DayOfWeek == DayOfWeek.Sunday)
                            {
                                Duration = ServiceHelper.CalculateProxyDuration(dtWorkingTimeIn, dtWorkingTimeOut, dtWorkingTimeIn, dtWorkingTimeOut, (DateTime?)dtWorkingDate, (DateTime?)dtWorkingDate, 0);

                                Assert.ThrowIf(Duration < 2 && Duration > 5 && cleanbdjkcodelist.Contains("B"), "Line " + counter + ": cannot insert. Duration of BDJK Code B must be between 2 - 5 hours");

                                Assert.ThrowIf(Duration < 5 && cleanbdjkcodelist.Contains("C"), "Line " + counter + ": cannot insert. Duration of BDJK Code C must be more than 5 hours");

                                Assert.ThrowIf(cleanbdjkcodelist.Contains("A"), "Line " + counter + ": cannot insert. BDJK Code A only for Weekday Date");
                            }
                            else
                            {
                                Duration = ServiceHelper.CalculateProxyDuration(dtWorkingTimeIn, dtWorkingTimeOut, dtWorkingTimeIn, dtWorkingTimeOut, normalSchedule.NormalTimeIn, normalSchedule.NormalTimeOut, 0);

                                Assert.ThrowIf(Duration < 2 && cleanbdjkcodelist.Contains("A"), "Line " + counter + ": cannot insert. Duration of BDJK Code A must be more than 2 hours");

                                Assert.ThrowIf(cleanbdjkcodelist.Contains("B"), "Line " + counter + ": cannot insert. BDJK Code B only for Weekend Date");

                                Assert.ThrowIf(cleanbdjkcodelist.Contains("C"), "Line " + counter + ": cannot insert. BDJK Code C only for Weekend Date");
                            }

                            workSheet.Cells[counter, 10].Value = Duration;

                            counter++;

                        } else
                        {
                            throw new Exception("Line " + counter + ": cannot insert. Format working date not correct");
                        }
                    } while (true);
                    workSheet.Cells[1, 1].Value = "NoReg";
                    workSheet.Cells[1, 2].Value = "BdjkCode";
                    workSheet.Cells[1, 3].Value = "BdjkReason";
                    workSheet.Cells[1, 4].Value = "ActivityCode";
                    workSheet.Cells[1, 5].Value = "WorkingDate";
                    workSheet.Cells[1, 6].Value = "WorkingTimeIn";
                    workSheet.Cells[1, 7].Value = "WorkingTimeOut";
                    workSheet.Cells[1, 8].Value = "Taxi";
                    workSheet.Cells[1, 9].Value = "UangMakanDinas";
                    workSheet.Cells[1, 10].Value = "BdjkDuration";
                    await UploadAndMergeAsync<AbnormalityBdjk>(workSheet, dicts, ComparerKeys, excludes);
                }
            }

            return NoContent();
        }

        [HttpGet("download-report")]
        public IActionResult DownloadReport(DateTime startDate, DateTime endDate, string name)
        {
            var NoReg = ServiceProxy.UserClaim.NoReg;
            var PostCode = ServiceProxy.UserClaim.PostCode;

            name = System.Web.HttpUtility.HtmlEncode(name);

            DataSourceRequest dataSourceRequest = new DataSourceRequest();

            dataSourceRequest.Filters = new List<IFilterDescriptor>();

            FilterDescriptor startDateFilter = new FilterDescriptor();
            startDateFilter.Member = "WorkingDate";
            startDateFilter.Operator = FilterOperator.IsGreaterThanOrEqualTo;
            startDateFilter.Value = startDate;
            dataSourceRequest.Filters.Add(startDateFilter);

            FilterDescriptor endDateFilter = new FilterDescriptor();
            endDateFilter.Member = "WorkingDate";
            endDateFilter.Operator = FilterOperator.IsLessThanOrEqualTo;
            endDateFilter.Value = endDate;
            dataSourceRequest.Filters.Add(endDateFilter);

            if (name != null)
            {
                var employeeName = name.Split('-')[1].Trim();
                var noReg = name.Split('-')[0].Trim();

                FilterDescriptor employeeNameFilter = new FilterDescriptor();
                employeeNameFilter.Member = "Name";
                employeeNameFilter.Operator = FilterOperator.Contains;
                employeeNameFilter.Value = employeeName;
                dataSourceRequest.Filters.Add(employeeNameFilter);

                FilterDescriptor noRegFilter = new FilterDescriptor();
                noRegFilter.Member = "NoReg";
                noRegFilter.Operator = FilterOperator.Contains;
                noRegFilter.Value = noReg;
                dataSourceRequest.Filters.Add(noRegFilter);
            }

            var getData = abnormalityBdjkService.GetsMasterDataBdjkView().ToDataSourceResult(dataSourceRequest);

            var fileName = string.Format("TM MASTER DATA BDJK {0:ddMMyyyy}-{1:ddMMyyyy}_{2:yyyyMMDD}.xlsx", startDate, endDate, DateTime.Now);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;

                    var sheet = package.Workbook.Worksheets.Add("Output");

                    var cols = new[] { "No Reg", "Name", "Bdjk Code", "Bdjk Reason", "Activity Code", "Working Date", "Working Time In", "Working Time Out", "Taxi", "Uang Makan Dinas", "Bdjk Duration", "Created Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        sheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                    }

                    foreach (MasterDataBdjkView item in getData.Data)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.NoReg;
                        sheet.Cells[rowIndex, 2].Value = item.Name;

                        var ls = item.BdjkCode.Split(' ');
                        string bdjkCode = "";
                        foreach(string s in ls)
                        {
                            if(s != "T" && s != "D")
                            {
                                bdjkCode += s;
                            }
                        }
                        sheet.Cells[rowIndex, 3].Value = bdjkCode;
                        sheet.Cells[rowIndex, 4].Value = item.BdjkReason;
                        sheet.Cells[rowIndex, 5].Value = item.ActivityCode;
                        sheet.Cells[rowIndex, 6].Value = item.WorkingDate.ToString("dd/MM/yyyy");
                        sheet.Cells[rowIndex, 7].Value = item.WorkingTimeIn.HasValue ? item.WorkingTimeIn.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 8].Value = item.WorkingTimeOut.HasValue ? item.WorkingTimeOut.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                        sheet.Cells[rowIndex, 9].Value = item.Taxi.Value;
                        sheet.Cells[rowIndex, 10].Value = item.UangMakanDinas.Value;
                        sheet.Cells[rowIndex, 11].Value = item.Duration;
                        sheet.Cells[rowIndex, 12].Value = item.CreatedOn.ToString("dd/MM/yyyy");

                        for (var i = 1; i <= cols.Length; i++)
                        {
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

        [HttpGet("multiple/download-template")]
        public IActionResult DownloadMultipleTemplate(DateTime? keyDate)
        {
            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var date = keyDate ?? DateTime.Now;
                    var totalDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);
                    var startDate = new DateTime(date.Year, date.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);
                    var culture = new CultureInfo(ApplicationConstants.IndoCultureInfo);
                    var name = "BDJK-MULTIPLE-TEMPLATE.xlsx";
                    var sheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(name));
                    var month = date.ToString("MMM yyyy", culture);

                    var request = new DataSourceRequest
                    {
                        Filters = new List<IFilterDescriptor>
                        {
                            new FilterDescriptor("WorkingDate", FilterOperator.IsGreaterThanOrEqualTo, startDate),
                            new FilterDescriptor("WorkingDate", FilterOperator.IsLessThanOrEqualTo, endDate)
                        },
                                Sorts = new List<SortDescriptor>
                        {
                            new SortDescriptor("WorkingDate", ListSortDirection.Ascending),
                            new SortDescriptor("Name", ListSortDirection.Ascending)
                        }
                    };

                    var bdjkData = timeEvaluationService.GenerateBDJKEvaluations(keyDate.Value.Month, keyDate.Value.Year);

                    int rowIndex = 3;

                    var cols = new[] { "No Reg. ", "Name", "A", "B", "C", "D", "T", "A", "B", "C", "D", "T", "A", "B", "C", "D", "T" };

                    sheet.Cells[1, 1].Value = cols[0];
                    sheet.Cells[1, 1].Style.Font.Bold = true;
                    sheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, 1, 2, 1].Merge = true;
                    sheet.Cells[1, 1, 2, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    sheet.Cells[1, 2].Value = cols[1];
                    sheet.Cells[1, 2].Style.Font.Bold = true;
                    sheet.Cells[1, 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[1, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, 2, 2, 2].Merge = true;
                    sheet.Cells[1, 2, 2, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    sheet.Cells[1, 3].Value = month;
                    sheet.Cells[1, 3].Style.Font.Bold = true;
                    sheet.Cells[1, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[1, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    sheet.Cells[1, 3, 1, 7].Merge = true;
                    sheet.Cells[1, 3, 1, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    sheet.Cells[1, 8].Value = "Abnormality / Master BDJK";
                    sheet.Cells[1, 8].Style.Font.Bold = true;
                    sheet.Cells[1, 8].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[1, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    sheet.Cells[1, 8, 1, 12].Merge = true;
                    sheet.Cells[1, 8, 1, 12].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    sheet.Cells[1, 13].Value = "Total";
                    sheet.Cells[1, 13].Style.Font.Bold = true;
                    sheet.Cells[1, 13].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[1, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    sheet.Cells[1, 13, 1, 17].Merge = true;
                    sheet.Cells[1, 13, 1, 17].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    for (var i = 3; i <= cols.Length; i++)
                    {
                        sheet.Cells[2, i].Value = cols[i - 1];
                        sheet.Cells[2, i].Style.Font.Bold = true;
                        sheet.Cells[2, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        sheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Cells[2, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        if(i>=8 && i <= 12)
                        {
                            sheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        } else if(i >= 13){
                            sheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        } else
                        {
                            sheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                        }
                        sheet.Cells[2, i].Style.Font.Color.SetColor(Color.White);
                    }

                    foreach (var item in bdjkData)
                    {
                        sheet.Cells[rowIndex, 1].Value = item.NoReg;
                        sheet.Cells[rowIndex, 2].Value = item.Name;
                        sheet.Cells[rowIndex, 3].Value = item.A;
                        sheet.Cells[rowIndex, 4].Value = item.B;
                        sheet.Cells[rowIndex, 5].Value = item.C;
                        sheet.Cells[rowIndex, 6].Value = item.D;
                        sheet.Cells[rowIndex, 7].Value = item.T;
                        sheet.Cells[rowIndex, 8].Value = item.AA;
                        sheet.Cells[rowIndex, 9].Value = item.AB;
                        sheet.Cells[rowIndex, 10].Value = item.AC;
                        sheet.Cells[rowIndex, 11].Value = item.AD;
                        sheet.Cells[rowIndex, 12].Value = item.AT;
                        sheet.Cells[rowIndex, 13].Value = item.A + item.AA;
                        sheet.Cells[rowIndex, 14].Value = item.B + item.AB;
                        sheet.Cells[rowIndex, 15].Value = item.C + item.AC;
                        sheet.Cells[rowIndex, 16].Value = item.D + item.AD;
                        sheet.Cells[rowIndex, 17].Value = item.T + item.AT;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    package.SaveAs(ms);

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
                }
            }
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Employee leave page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewBdjkMasterData)]
    public class BdjkMvcController : GenericMvcControllerBase<AbnormalityBdjkService, AbnormalityBdjk>
    {
    }
    #endregion
} 