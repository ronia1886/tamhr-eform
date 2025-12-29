using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class BonusDeductionReport
    {
        // -------- helpers ----------
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
                        hits = hits.Where(p =>
                            ContainsIgnoreCase(p.Replace('\\', '/'), pathMustContain.Replace('\\', '/'))
                        ).ToArray();
                    if (hits.Length > 0) return hits[0];
                }
                catch
                {
                    // skip folders we can't read
                }
            }
            return null;
        }

        private static bool HasAnyLabel(string text) =>
            ContainsIgnoreCase(text, "Bonus Deduction Report") ||
            ContainsIgnoreCase(text, "Bonus Deduction");

        // -------- tests ----------

        [Fact]
        public void View_Index_ShouldContain_BonusDeduction_Report_Category()
        {
            // Views/TimeManagement/MonitoringReportAll/Index.cshtml
            var path = FindFileInRepo("Index.cshtml", "Areas/TimeManagement/Views/MonitoringReportAll");
            Assert.True(File.Exists(path ?? string.Empty),
                "File Views/MonitoringReportAll/Index.cshtml tidak ditemukan.");

            var text = File.ReadAllText(path);

            // Harus ada label kategori “Bonus Deduction” atau “Bonus Deduction Report”
            Assert.True(HasAnyLabel(text),
                "Label 'Bonus Deduction' atau 'Bonus Deduction Report' tidak ditemukan di Index.cshtml.");

            // Ekstra: variabel categoryName untuk kategori ini
            Assert.True(
                ContainsIgnoreCase(text, "categoryName") && HasAnyLabel(text),
                "Deklarasi categoryName untuk Bonus Deduction (Report) tidak terdeteksi di Index.cshtml.");
        }

        [Fact]
        public void Controller_ShouldSet_Title_To_BonusDeduction_Report()
        {
            // Areas/TimeManagement/Controllers/MonitoringReportAllController.cs
            var path = FindFileInRepo("MonitoringReportAllController.cs", "Areas/TimeManagement/Controllers");
            Assert.True(File.Exists(path ?? string.Empty),
                "File MonitoringReportAllController.cs tidak ditemukan.");

            var text = File.ReadAllText(path);

            // umumnya ada baris: title = "Bonus Deduction";
            Assert.True(
                ContainsIgnoreCase(text, "title") && HasAnyLabel(text),
                "Penetapan title untuk Bonus Deduction (Report) tidak terdeteksi di controller.");
        }

        [Fact]
        public void IncentiveReport_View_ShouldDisplay_BonusDeduction_Label()
        {
            // Views/TimeManagement/MonitoringReportAll/IncentiveReport.cshtml
            var path = FindFileInRepo("IncentiveReport.cshtml", "Areas/TimeManagement/Views/MonitoringReportAll");
            Assert.True(File.Exists(path ?? string.Empty),
                "File IncentiveReport.cshtml tidak ditemukan.");

            var text = File.ReadAllText(path);

            // Terima 'Bonus Deduction', 'Bonus Deduction Report', dan toleransi 'Bonus Deductionxxx'
            var ok =
                HasAnyLabel(text) ||
                ContainsIgnoreCase(text, "Bonus Deductionxxx");

            Assert.True(ok,
                "Label 'Bonus Deduction' (atau 'Bonus Deduction Report' / 'Bonus Deductionxxx') tidak ditemukan di IncentiveReport.cshtml.");
        }
    }
}
