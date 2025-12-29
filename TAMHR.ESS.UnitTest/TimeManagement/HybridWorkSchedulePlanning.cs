using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class HybridWorkSchedulePlanning
    {
        // ---------- helpers ----------
        private static bool ContainsIgnoreCase(string text, string needle) =>
            text?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string FindFileInRepo(string fileName, string pathMustContain = null)
        {
            // cari ke atas max 8 level dari working dir test
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                string[] hits;
                try
                {
                    hits = Directory.GetFiles(dir.FullName, fileName, SearchOption.AllDirectories);
                }
                catch
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(pathMustContain))
                {
                    var norm = pathMustContain.Replace('\\', '/').ToLowerInvariant();
                    hits = hits.Where(p => p.Replace('\\', '/').ToLowerInvariant().Contains(norm)).ToArray();
                }

                if (hits.Length > 0) return hits[0];
            }
            return null;
        }

        // ---------- tests ----------

        [Fact]
        public void EmailService_Should_Format_Planning_Reminder_Summary_FileName()
        {
            // Infrastructure/Modules/Core/DomainServices/EmailService.cs
            var path = FindFileInRepo("EmailService.cs", "Infrastructure/Modules/Core/DomainServices");
            Assert.True(File.Exists(path ?? string.Empty),
                "EmailService.cs tidak ditemukan di path DomainServices.");

            var text = File.ReadAllText(path);

            // pola yang kamu temukan di source:
            // "Hybrid Work Schedule Planning Reminder Summary {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx"
            Assert.True(
                ContainsIgnoreCase(text, "Hybrid Work Schedule Planning Reminder Summary") &&
                ContainsIgnoreCase(text, ".xlsx"),
                "Pola nama file 'Hybrid Work Schedule Planning Reminder Summary ... .xlsx' tidak ditemukan.");
        }

        [Fact]
        public void WeeklyWFHPlanning_View_Should_Show_Planning_Title_And_Success_Messages()
        {
            // WebUI/Areas/TimeManagement/Views/Form/WeeklyWFHPlanning.cshtml
            var path = FindFileInRepo("WeeklyWFHPlanning.cshtml", "Areas/TimeManagement/Views/Form");
            Assert.True(File.Exists(path ?? string.Empty),
                "WeeklyWFHPlanning.cshtml tidak ditemukan.");

            var text = File.ReadAllText(path);

            // Title & pesan di view/script (berdasarkan snippet yang ada)
            var hasTitle = ContainsIgnoreCase(text, "Hybrid Work Schedule Plan");
            var hasSavedMsg =
                ContainsIgnoreCase(text, "Hybrid Work Schedule Plan Data saved successfully") ||
                ContainsIgnoreCase(text, "Hybrid Work Schedule Plan Data submitted successfully");

            Assert.True(hasTitle, "Title 'Hybrid Work Schedule Plan' tidak ditemukan di view.");
            Assert.True(hasSavedMsg, "Pesan sukses save/submit HWS Planning tidak ditemukan di view.");
        }

        [Fact]
        public void WeeklyWFHPlanning_Controller_Should_Contain_Success_Message_Strings()
        {
            // WebUI/Areas/TimeManagement/Controllers/WeeklyWFHPlanningController.cs
            var path = FindFileInRepo("WeeklyWFHPlanningController.cs", "Areas/TimeManagement/Controllers");
            Assert.True(File.Exists(path ?? string.Empty),
                "WeeklyWFHPlanningController.cs tidak ditemukan.");

            var text = File.ReadAllText(path);

            // cek string yang dicontohkan di controller
            Assert.True(
                ContainsIgnoreCase(text, "Hybrid Work Schedule Plan Data submitted successfully"),
                "Pesan 'submitted successfully' tidak ditemukan di controller.");
        }
    }
}
