using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.Payroll
{
    public class BupotServiceTest : IDisposable
    {
        private readonly string _tmpDir;

        public BupotServiceTest()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), "bupot_ut_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpDir);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tmpDir)) Directory.Delete(_tmpDir, true); } catch { }
        }

        // Helper: bikin PDF dummy
        private string CreateDummyPdf(string fileName = "bupot.pdf")
        {
            var path = Path.Combine(_tmpDir, fileName);
            var doc = new PdfDocument();
            doc.Info.Title = "UnitTest Bupot";
            doc.AddPage();
            using (var fs = File.Create(path))
                doc.Save(fs);
            return path;
        }

        // Helper: panggil private DownloadFromFile via reflection
        private static byte[] Call_DownloadFromFile(object svc, string localPath, string fileName,
                                                    string defaultPwdFile, string userPwd,
                                                    string ownerPwd, bool viewOnly)
        {
            var mi = typeof(BupotService)
                .GetMethod("DownloadFromFile", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(mi);

            var result = mi.Invoke(svc, new object[] { localPath, fileName, defaultPwdFile, userPwd, ownerPwd, viewOnly });
            return (byte[])result;
        }

        [Fact]
        public void DownloadFromFile_ViewOnly_True_ReturnsBytes_And_OpensWithoutPassword()
        {
            // Arrange: bikin file PDF dummy
            CreateDummyPdf("bupot.pdf");

            // Instance tanpa constructor (biar gak butuh ConfigService/PersonalDataService)
            var svc = (BupotService)FormatterServices.GetUninitializedObject(typeof(BupotService));

            // Act
            var bytes = Call_DownloadFromFile(
                svc,
                localPath: _tmpDir,
                fileName: "bupot.pdf",
                defaultPwdFile: "",      // source tanpa password
                userPwd: "user-secret",  // tak dipakai saat viewOnly=true
                ownerPwd: "owner-secret",
                viewOnly: true
            );

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 100);

            // ViewOnly=true → hasilnya tidak diproteksi, bisa dibuka tanpa password
            using var ms = new MemoryStream(bytes);
            var pdf = PdfReader.Open(ms, PdfDocumentOpenMode.ReadOnly);
            Assert.True(pdf.PageCount >= 1);
        }

        [Fact]
        public void DownloadFromFile_Throws_When_File_NotFound()
        {
            // Arrange
            var svc = (BupotService)FormatterServices.GetUninitializedObject(typeof(BupotService));

            // Act + Assert
            // Karena file tidak ada → internal Assert.ThrowIf(...) akan memicu exception
            Assert.ThrowsAny<Exception>(() =>
            {
                Call_DownloadFromFile(
                    svc,
                    localPath: _tmpDir,
                    fileName: "tidak-ada.pdf",
                    defaultPwdFile: "",
                    userPwd: "pw",
                    ownerPwd: "owner",
                    viewOnly: true
                );
            });
        }
    }
}
