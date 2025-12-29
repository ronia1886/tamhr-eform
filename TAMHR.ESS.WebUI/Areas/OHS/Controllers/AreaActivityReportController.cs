using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Data;
using OfficeOpenXml.Style;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace TAMHR.ESS.WebUI.Areas.OHS.Controllers
{
    #region API Controller
    /// <summary>
    /// API Area Activity Report OHS
    /// </summary>
    [Route("api/ohs/area-activity/report")]
    //[Permission(PermissionKey.ManageDailyWorkSchedule)]
    public class AreaActivityReportController : ApiControllerBase
    {
        public TotalEmployeeService TotalEmployeeService => ServiceProxy.GetService<TotalEmployeeService>();
        public SafetyIncidentService SafetyIncidentService => ServiceProxy.GetService<SafetyIncidentService>();
        public SafetyFacilityService SafetyFacilityService => ServiceProxy.GetService<SafetyFacilityService>();
        public FireProtectionService FireProtectionService => ServiceProxy.GetService<FireProtectionService>();
        public APARRefillService APARRefillService => ServiceProxy.GetService<APARRefillService>();
        public TrainingRecordService TrainingRecordService => ServiceProxy.GetService<TrainingRecordService>();
        public ProjectActivityService ProjectActivityService => ServiceProxy.GetService<ProjectActivityService>();
        public AreaService AreaService => ServiceProxy.GetService<AreaService>();

        public ConfigService GeneralCategory => ServiceProxy.GetService<ConfigService>();

        [HttpGet("download-report")]
        public async Task<IActionResult> AreaActivityExportToXlsxAsync(string? Periode, string? DivisionCode, string? AreaId, string? DivisionName)
        {
            string name = "Export_Area_Activity.xlsx";
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Area Activity Report");

                // URL gambar
                string logoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0c/Tam-logo.png";

                try
                {
                    byte[] logoBytes;

                    // Unduh gambar dari URL
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                        logoBytes = await client.GetByteArrayAsync(logoUrl);
                    }

                    // Tambahkan gambar ke worksheet menggunakan EPPlus
                    if (logoBytes != null)
                    {
                        using (var stream = new MemoryStream(logoBytes))
                        {
                            // Muat gambar menggunakan SixLabors.ImageSharp
                            using (var image = SixLabors.ImageSharp.Image.Load(stream))
                            {
                                // EPPlus memiliki metode untuk langsung menambahkan gambar dari MemoryStream
                                var picture = worksheet.Drawings.AddPicture("Logo", stream);

                                // Atur posisi dan ukuran gambar
                                picture.SetPosition(0, 0); // Posisi gambar (baris 1, kolom 1)
                                picture.SetSize(50, 50); // Ukuran gambar (lebar x tinggi dalam pixel)
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log atau tangani error jika unduh gambar gagal
                    Console.WriteLine($"Error saat memuat gambar: {ex.Message}");
                }

                // Konversi string Periode menjadi format "Nama Bulan - Tahun (yyyy)"
                string formattedPeriode;
                if (!string.IsNullOrEmpty(Periode))
                {
                    // Parse Periode menjadi DateTime
                    DateTime parsedPeriode = DateTime.ParseExact(Periode, "yyyy-MM", null);

                    // Format menjadi "Nama Bulan - Tahun"
                    formattedPeriode = parsedPeriode.ToString("MMMM - yyyy");
                }
                else
                {
                    formattedPeriode = "ALL PERIOD";
                }

                // Nama perusahaan dan sekretariat
                worksheet.Cells[1, 2].Value = "PT TOYOTA-ASTRA MOTOR";
                worksheet.Cells[1, 2].Style.Font.Size = 10;
                worksheet.Cells[1, 2].Style.Font.Bold = true;

                worksheet.Cells[2, 2].Value = "OHS SECRETARIAT";
                worksheet.Cells[2, 2].Style.Font.Size = 10;

                // Kolom periode dan tanggal
                worksheet.Cells[4, 1].Value = "MONTH:";
                worksheet.Cells[4, 1].Style.Font.Bold = true;

                worksheet.Cells[4, 2].Value = formattedPeriode;
                worksheet.Cells[4, 2].Style.Font.Bold = true;

                worksheet.Cells[4, 6].Value = "DATE:";
                worksheet.Cells[4, 6].Style.Font.Bold = true;

                worksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd-MM-yyyy");
                worksheet.Cells[4, 7].Style.Font.Bold = true;

                // Judul laporan besar
                var safeDivisionName = (DivisionName ?? "")
                                        .Replace("<", "")
                                        .Replace(">", "")
                                        .Replace("\"", "")
                                        .Replace("'", "");
                worksheet.Cells[6, 1].Value = $"'OHS MONTHLY REPORT - {(string.IsNullOrEmpty(DivisionName) ? "ALL DIVISION" : safeDivisionName)}";
                worksheet.Cells[6, 1, 6, 10].Merge = true;
                worksheet.Cells[6, 1].Style.Font.Size = 16;
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Style.QuotePrefix = true;
                worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Header tanda tangan
                int signatureStartColumn = 9; // Kolom awal untuk tanda tangan
                int signatureRowHeight = 30; // Tinggi untuk baris tanda tangan

                // Kolom tanda tangan: Department Head
                worksheet.Cells[1, signatureStartColumn].Value = "Department Head";
                worksheet.Cells[1, signatureStartColumn].Style.Font.Size = 10;
                worksheet.Cells[1, signatureStartColumn].Style.Font.Bold = true;

                // Atur rata tengah untuk header "Department Head"
                worksheet.Cells[1, signatureStartColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, signatureStartColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Gabungkan sel untuk ruang tanda tangan
                worksheet.Cells[2, signatureStartColumn, 3, signatureStartColumn].Merge = true;
                worksheet.Row(3).Height = signatureRowHeight; // Tinggi baris tanda tangan

                // Atur rata tengah untuk ruang tanda tangan
                worksheet.Cells[2, signatureStartColumn, 3, signatureStartColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, signatureStartColumn, 3, signatureStartColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Tambahkan nama di bawah tanda tangan
                worksheet.Cells[4, signatureStartColumn].Value = "Edwin Budianto";
                worksheet.Cells[4, signatureStartColumn].Style.Font.Size = 10;
                worksheet.Cells[4, signatureStartColumn].Style.Font.Bold = true;

                // Atur rata tengah untuk nama
                worksheet.Cells[4, signatureStartColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, signatureStartColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                // Tambahkan border pada kolom Department Head
                using (var range = worksheet.Cells[1, signatureStartColumn, 4, signatureStartColumn])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Kolom tanda tangan: Section Head
                worksheet.Cells[1, signatureStartColumn + 1].Value = "Section Head";
                worksheet.Cells[1, signatureStartColumn + 1].Style.Font.Size = 10;
                worksheet.Cells[1, signatureStartColumn + 1].Style.Font.Bold = true;

                worksheet.Cells[2, signatureStartColumn + 1, 3, signatureStartColumn + 1].Merge = true; // Gabungkan sel untuk ruang tanda tangan
                worksheet.Row(3).Height = signatureRowHeight; // Tinggi baris tanda tangan
                worksheet.Cells[4, signatureStartColumn + 1].Value = "Abdul Ghofur";
                worksheet.Cells[4, signatureStartColumn + 1].Style.Font.Size = 10;
                worksheet.Cells[4, signatureStartColumn].Style.Font.Bold = true;

                // Tambahkan border pada kolom Section Head
                using (var range = worksheet.Cells[1, signatureStartColumn + 1, 4, signatureStartColumn + 1])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                int currentRow = 8; // Memulai tabel setelah header
                var Noreg = ServiceProxy.UserClaim.NoReg;
                // Deklarasi dan inisialisasi variabel
                var listNearMiss = SafetyIncidentService.GetSafetyAccident("near_miss", Periode, DivisionCode, AreaId, Noreg).ToList();
                var listPropertyDamage = SafetyIncidentService.GetSafetyAccident("property_damage", Periode, DivisionCode, AreaId, Noreg).ToList();
                var listWorkingAccident = SafetyIncidentService.GetSafetyAccident("working_accident", Periode, DivisionCode, AreaId, Noreg).ToList();
                var listTrafficAccident = SafetyIncidentService.GetSafetyAccident("traffic_accident", Periode, DivisionCode, AreaId, Noreg).ToList();
                var listSafetyFacility = SafetyFacilityService.GetSafetyFacility(Periode, DivisionCode, AreaId, Noreg).ToList();
                var listFireProtection = FireProtectionService.GetFireProtection(Periode, DivisionCode, AreaId, Noreg).ToList();
                var listAPARRefill = APARRefillService.GetAPARRefill(Periode, DivisionCode, AreaId, Noreg).ToList();

                var listTrainingRecord = TrainingRecordService.GetTrainingRecord(Periode, DivisionCode, AreaId).ToList();
                var listProjectActivity = ProjectActivityService.GetProjectActivity(Periode, DivisionCode, AreaId).ToList();
                var listTotalWorker = TotalEmployeeService.GetTotalEmployeeSummary(Periode, DivisionCode, AreaId);

                // Membuat tabel
                var tables = new List<(string Title, List<string> Columns, List<dynamic> Data)>
        {
            (
                "Jumlah Karyawan dan Outourcing",
                new List<string> { "No", "Description", "Value" },
                new List<dynamic>
                {
                    new { No = 1, Description = "Total Employee", Value = listTotalWorker.TotalEmployee },
                    new { No = 2, Description = "Total Outsourcing", Value = listTotalWorker.TotalEmployeeOutsourcing },
                    new { No = 3, Description = "Total", Value = listTotalWorker.TotalEmployee + listTotalWorker.TotalEmployeeOutsourcing }
                }
            ),
            (
                "Near Miss",
                new List<string> { "No", "Incident Description", "Area", "Day/Date", "Time", "Subject", "Remarks" },
                listNearMiss.Select((item, index) => new
                {
                    No = index + 1,
                    IncidentDescription = item.IncidentDescription,
                    Area = item.AreaName,
                    DayDate = item.IncidentDate.ToString("dd-MM-yy"),
                    Time = item.IncidentDate.ToString("HH:mm"),
                    Subject = item.Subject,
                    Remarks = item.Remark
                }).Cast<dynamic>().ToList()
            ),
            (
                "Property Damage",
                new List<string> { "No", "Incident Description", "Area", "Day/Date", "Time", "Property Type", "Total Loss (rupiah)", "Remarks" },
                listPropertyDamage.Select((item, index) => new
                {
                    No = index + 1,
                    IncidentDescription = item.IncidentDescription,
                    Area = item.AreaName,
                    DayDate = item.IncidentDate.ToString("dd-MM-yy"),
                    Time = item.IncidentDate.ToString("HH:mm"),
                    PropertyType = item.PropertyType,
                    TotalLoss = item.TotalLoss,
                    Remarks = item.Remark
                }).Cast<dynamic>().ToList()
            ),
            (
                "Working Accident",
                new List<string> { "No", "Incident Description", "Area", "Day/Date", "Time", "Subject", "Accident Type", "Total Victim (person)", "Loss Time (day)", "Total Loss (rupiah)" },
                listWorkingAccident.Select((item, index) => new
                {
                    No = index + 1,
                    IncidentDescription = item.IncidentDescription,
                    Area = item.AreaName,
                    DayDate = item.IncidentDate.ToString("dd-MM-yy"),
                    Time = item.IncidentDate.ToString("HH:mm"),
                    Subject = item.Subject,
                    AccidentType = item.AccidentType,
                    TotalVictim = item.TotalVictim ?? 0, // Pastikan nilai tidak null
                    LossTime = item.LossTime ?? "0", // Pastikan nilai tidak null
                    TotalLoss = item.TotalLoss ?? "0" // Pastikan nilai tidak null
                }).Cast<dynamic>().ToList()
            ),
            (
                "Traffic Accident",
                new List<string> { "No", "Incident Description", "Area", "Day/Date", "Time", "Subject", "Accident Type", "Total Victim (person)", "Total Loss (rupiah)" },
                listTrafficAccident.Select((item, index) => new
                {
                    No = index + 1,
                    IncidentDescription = item.IncidentDescription,
                    Area = item.AreaName,
                    DayDate = item.IncidentDate.ToString("dd-MM-yy"),
                    Time = item.IncidentDate.ToString("HH:mm"),
                    Subject = item.Subject,
                    AccidentType = item.AccidentType,
                    TotalVictim = item.TotalVictim ?? 0, // Pastikan nilai tidak null
                    TotalLoss = item.TotalLoss ?? "0" // Pastikan nilai tidak null
                }).Cast<dynamic>().ToList()
            ),
            (
                "Safety Facility",
                new List<string> { "No", "Equipment Name", "Area", "TotalPlan", "TotalActual", "Remark" },
                listSafetyFacility.Select((item, index) => new
                {
                    No = index + 1,
                    EquipmentName = item.EquipmentName,
                    Area = item.AreaName,
                    TotalPlan = item.TotalPlan,
                    TotalActual = item.TotalActual,
                    Remark = item.Remark
                }).Cast<dynamic>().ToList()
            ),
            (
                "Fire Protection",
                new List<string> { "No", "Installed", "Ready", "Readiness", "Division Name", "Area Name", "Category" },
                listFireProtection.Select((item, index) => new
                {
                    No = index + 1,
                    Installed = item.Installed,
                    Ready = item.Ready,
                    Readiness = item.Readiness,
                    DivisionName = item.DivisionName,
                    AreaName = item.AreaName,
                    Category = item.Category
                }).Cast<dynamic>().ToList()
            ),
            (
                "APAR Refill",
                new List<string> { "No", "Merk", "Type", "Qty", "Division Name", "Area Name", "Remark" },
                listAPARRefill.Select((item, index) => new
                {
                     No = index + 1,
                    Merk = item.Merk,
                    Type = item.Type,
                    Qty = item.Qty,
                    DivisionName = item.DivisionName,
                    AreaName = item.AreaName,
                    Remark = item.Remark
                }).Cast<dynamic>().ToList()
            ),
            (
                "Training Record",
                new List<string> { "No", "Training Description", "Training Institution", "Area Name",  "Training Start Date", "Training End Date", "Participant",  "Remark" },
                listTrainingRecord.Select((item, index) => new
                {
                     No = index + 1,
                    Description = item.Description,
                    Institution = item.Institution,
                    AreaName = item.AreaName,
                    DateStart = item.DateStart.ToString("dd-MM-yy"),
                    DateEnd = item.DateEnd.HasValue ? item.DateEnd.Value.ToString("dd-MM-yy") : "",
                    Participant = item.Participant,
                    Remark = item.Remark
                }).Cast<dynamic>().ToList()
            ),
            (
                "Project Activity",
                new List<string> { "No", "Contractor", "Area Name", "Project Name", "Start Date", "End Date", "Total Worker" },
                listProjectActivity.Select((item, index) => new
                {
                     No = index + 1,
                    Contractor = item.Contractor,
                    AreaName = item.AreaName,
                    ProjectName = item.ProjectName,
                    StartDate = item.StartDate.ToString("dd-MM-yy"),
                    EndDate = item.EndDate.ToString("dd-MM-yy"),
                    TotalWorker = item.TotalWorker
                }).Cast<dynamic>().ToList()
            )
        };

                // Generate tables
                foreach (var table in tables)
                {
                    worksheet.Cells[currentRow, 1].Value = table.Title;
                    worksheet.Cells[currentRow, 1, currentRow, table.Columns.Count].Merge = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    currentRow++;

                    // Add table headers
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        var cell = worksheet.Cells[currentRow, col + 1];
                        cell.Value = table.Columns[col];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;

                    // Add table rows
                    foreach (var row in table.Data)
                    {
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            var columnName = table.Columns[col];
                            var propertyName = columnName.Replace(" ", "").Replace("/", "")
                                .Replace("(day)", "")
                                .Replace("(rupiah)", "")
                                .Replace("(person)", "")
                                .Replace("TrainingStartDate", "DateStart")
                                .Replace("TrainingEndDate", "DateEnd")
                                .Replace("Training-", "Training")
                                .Replace("Training", "");

                            var property = row.GetType().GetProperty(propertyName);
                            if (property == null)
                            {
                                worksheet.Cells[currentRow, col + 1].Value = ""; // Jika properti tidak ditemukan
                                continue;
                            }
                            var value = property.GetValue(row, null);
                            worksheet.Cells[currentRow, col + 1].Value = value?.ToString() ?? "";
                            worksheet.Cells[currentRow, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        currentRow++;
                    }

                    currentRow += 2; // Add spacing between tables
                }

                worksheet.Cells.AutoFitColumns();

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    memoryStream.Position = 0;
                    return File(memoryStream.ToArray(), contentType, name);
                }
            }
        }
    }
}
#endregion

#region MVC Controller
/// <summary>
/// OHS Area Activity Controller
/// </summary>

#endregion