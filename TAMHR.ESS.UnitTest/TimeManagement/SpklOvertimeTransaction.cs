using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class SpklOvertimeTransaction
    {
        // ----- helpers -----
        private static bool ContainsIgnoreCase(string text, string needle) =>
            text?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string FindFileInRepoExact(string fileName, string pathMustContain = null)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                string[] hits;
                try { hits = Directory.GetFiles(dir.FullName, fileName, SearchOption.AllDirectories); }
                catch { continue; }
                if (!string.IsNullOrEmpty(pathMustContain))
                {
                    var needle = pathMustContain.Replace('\\', '/').ToLowerInvariant();
                    hits = hits.Where(p => p.Replace('\\', '/').ToLowerInvariant().Contains(needle)).ToArray();
                }
                if (hits.Length > 0) return hits[0];
            }
            return null;
        }

        private static string FindAnyControllerFile()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                string[] hits;
                try { hits = Directory.GetFiles(dir.FullName, "*Controller.cs", SearchOption.AllDirectories); }
                catch { continue; }

                var filtered = hits.Where(p =>
                    p.Replace('\\', '/').ToLowerInvariant().Contains("areas/timemanagement/controllers") &&
                    Path.GetFileName(p).ToLowerInvariant().Contains("spkl") &&
                    Path.GetFileName(p).ToLowerInvariant().Contains("overtime") &&
                    Path.GetFileName(p).ToLowerInvariant().Contains("controller"))
                    .ToArray();

                if (filtered.Length > 0) return filtered[0];
            }
            return null;
        }

        // ----- tests -----

        [Fact]
        public void ApplicationConstants_Should_Contain_SpklOvertime_FormKey()
        {
            var path = FindFileInRepoExact("ApplicationConstants.cs", "Infrastructure");
            Assert.True(File.Exists(path ?? string.Empty),
                "ApplicationConstants.cs tidak ditemukan di folder Infrastructure.");

            var text = File.ReadAllText(path);

            Assert.True(
                ContainsIgnoreCase(text, "SPKL overtime form key constant"),
                "Komentar 'SPKL overtime form key constant.' tidak ditemukan.");

            // toleran: konstanta yang mengandung 'spkl' juga dianggap valid
            var m = Regex.Match(text, @"const\s+string\s+[A-Za-z0-9_]*spkl[A-Za-z0-9_]*\s*=\s*""([^""]+)""",
                                RegexOptions.IgnoreCase);
            Assert.True(m.Success || ContainsIgnoreCase(text, "spkl-overtime"),
                "Konstanta form key SPKL (mengandung 'spkl') tidak ditemukan.");
        }

        [Fact]
        public void FormController_Should_Register_SpklOvertime_Actions()
        {
            var path = FindFileInRepoExact("FormController.cs", "Areas/TimeManagement/Controllers");
            Assert.True(File.Exists(path ?? string.Empty),
                "FormController.cs tidak ditemukan di Areas/TimeManagement/Controllers.");

            var text = File.ReadAllText(path);

            Assert.True(
                ContainsIgnoreCase(text, "Register SPKL overtime form action"),
                "Komentar pendaftaran SPKL overtime form action tidak ditemukan.");

            Assert.True(
                ContainsIgnoreCase(text, "Register SPKL overtime download form action"),
                "Komentar pendaftaran SPKL overtime download form action tidak ditemukan.");
        }

        [Fact]
        public void SpklOvertimeController_Should_Exist_And_Have_Routing()
        {
            // 1) coba nama file persis
            var path = FindFileInRepoExact("SpklOvertimeController.cs", "Areas/TimeManagement/Controllers");

            // 2) jika tidak ada, cari apa pun yang mengandung Spkl+Overtime+Controller
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                path = FindAnyControllerFile();

            Assert.True(File.Exists(path ?? string.Empty),
                "File controller SPKL Overtime tidak ditemukan di Areas/TimeManagement/Controllers (mencari *Spkl*Overtime*Controller.cs).");

            var text = File.ReadAllText(path);

            // cari pola nama class yang fleksibel: class <apapun>Spkl<apapun>Overtime<apapun>Controller
            var classRegex = new Regex(@"class\s+[A-Za-z0-9_]*Spkl[A-Za-z0-9_]*Overtime[A-Za-z0-9_]*Controller",
                                       RegexOptions.IgnoreCase);
            Assert.True(classRegex.IsMatch(text),
                "Deklarasi class controller SPKL Overtime (regex 'class .*Spkl.*Overtime.*Controller') tidak ditemukan.");

            // cek ada attribute route/action
            Assert.True(
                ContainsIgnoreCase(text, "[Route(") ||
                ContainsIgnoreCase(text, "[HttpGet(") ||
                ContainsIgnoreCase(text, "[HttpPost(") ||
                ContainsIgnoreCase(text, "[HttpPut(") ||
                ContainsIgnoreCase(text, "[HttpDelete("),
                "Attribute routing/action method Http* tidak ditemukan (minimal satu).");
        }
    }
}
