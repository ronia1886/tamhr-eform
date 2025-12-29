using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class AttendanceIncentiveReport
    {
        // --- helpers ---
        private static bool ContainsIgnoreCase(string haystack, string needle) =>
            haystack?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string FindFileInRepo(string fileName, string pathMustContain = null)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                try
                {
                    var hits = Directory.GetFiles(dir.FullName, fileName, SearchOption.AllDirectories);
                    if (!string.IsNullOrEmpty(pathMustContain))
                        hits = hits.Where(p => ContainsIgnoreCase(p.Replace('\\', '/'), pathMustContain.Replace('\\', '/'))).ToArray();
                    if (hits.Length > 0) return hits[0];
                }
                catch { /* skip inaccessible folders */ }
            }
            return null;
        }

        private static bool HasAnyLabel(string text) =>
            ContainsIgnoreCase(text, "Attendance Incentive Report") ||
            ContainsIgnoreCase(text, "Attendance Incentive");

        // --- tests ---

        [Fact]
        public void View_Index_ShouldContain_AttendanceIncentive_Report_Label()
        {
            // Views/TimeManagement/MonitoringReportAll/Index.cshtml
            var path = FindFileInRepo("Index.cshtml", "Areas/TimeManagement/Views/MonitoringReportAll");
            Assert.True(File.Exists(path ?? string.Empty),
                "File Views/MonitoringReportAll/Index.cshtml tidak ditemukan.");

            var text = File.ReadAllText(path);
            Assert.True(HasAnyLabel(text),
                "Label 'Attendance Incentive' atau 'Attendance Incentive Report' tidak ditemukan di Index.cshtml.");

            // ekstra: khusus variabel kategori
            Assert.True(
                ContainsIgnoreCase(text, "categoryName") && HasAnyLabel(text),
                "Deklarasi categoryName untuk Attendance Incentive (Report) tidak terdeteksi di Index.cshtml.");
        }

        [Fact]
        public void Controller_ShouldContain_AttendanceIncentive_Report_Label()
        {
            // Areas/TimeManagement/Controllers/MonitoringReportAllController.cs
            var path = FindFileInRepo("MonitoringReportAllController.cs", "Areas/TimeManagement/Controllers");
            Assert.True(File.Exists(path ?? string.Empty),
                "File MonitoringReportAllController.cs tidak ditemukan.");

            var text = File.ReadAllText(path);
            Assert.True(HasAnyLabel(text),
                "Label 'Attendance Incentive' atau 'Attendance Incentive Report' tidak ditemukan di MonitoringReportAllController.cs.");

            // ekstra: sering diset via 'string title = "Attendance Incentive"'
            Assert.True(
                ContainsIgnoreCase(text, "title") && HasAnyLabel(text),
                "Penetapan title untuk Attendance Incentive (Report) tidak terdeteksi di controller.");
        }
    }
}
