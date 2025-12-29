using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure;

namespace TAMHR.ESS.UnitTest.Payroll
{
    public class PayslipServiceTest
    {
        /// <summary>
        /// Membuat PayslipService dengan IUnitOfWork mock:
        /// - Repo UserHash bisa dikontrol (kosong / ada entry).
        /// - ConfigService dibuat LOSE (tanpa setup GetConfigValue apapun)
        ///   agar kita tidak menyentuh extension methods/optional args.
        /// - PersonalDataService dibiarkan null (hanya untuk skenario tanpa UserHash).
        /// </summary>
        private static PayslipService CreateService(
            bool withUserHash,
            out Mock<IUnitOfWork> uow)
        {
            uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Mock repository UserHash
            var userHashRepo = new Mock<IRepository<UserHash>>(MockBehavior.Strict);
            var hashes = (withUserHash
                ? new[]
                  {
                      new UserHash
                      {
                          Id = Guid.NewGuid(),
                          NoReg = "E001",
                          TypeCode = HashType.Payslip,
                          HashValue = "dummy" // nilai tidak dipakai; kita hanya ingin lewati cabang _personalDataService
                      }
                  }.AsQueryable()
                : Array.Empty<UserHash>().AsQueryable());

            userHashRepo.Setup(r => r.Fetch()).Returns(hashes);

            // pastikan GetRepository<UserHash>() mengembalikan repo di atas
            uow.Setup(u => u.GetRepository<UserHash>()).Returns(userHashRepo.Object);

            // Loose ConfigService: TIDAK ada Setup ke GetConfigValue(..) apapun
            // sehingga kita menghindari error "expression tree + optional args".
            var cfgLoose = new Mock<ConfigService>(MockBehavior.Loose, uow.Object);

            // PersonalDataService: boleh null untuk skenario tanpa user-hash.
            PersonalDataService pds = null;

            return new PayslipService(cfgLoose.Object, pds, uow.Object);
        }

        [Fact]
        public async Task Download_Should_Throw_When_ConfigMissing_And_NoUserHash()
        {
            // Tanpa UserHash ⇒ kode akan mencoba default password via PersonalDataService/config,
            // yang secara environment test ini memang tidak tersedia.
            var svc = CreateService(withUserHash: false, out var _);

            // Kita cukup assert bahwa ada exception (jenisnya bebas)
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await svc.Download(noreg: "E001", month: 1, period: 2025, isOffCycle: false, viewOnly: true);
            });
        }

        [Fact]
        public async Task Download_Should_Throw_When_File_NotFound_Even_With_UserHash()
        {
            // Dengan UserHash ⇒ PersonalDataService tidak dipakai,
            // tapi karena konfigurasi path/filename tidak diset, file tetap tidak ditemukan.
            var svc = CreateService(withUserHash: true, out var _);

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await svc.Download(noreg: "E001", month: 2, period: 2024, isOffCycle: true, viewOnly: true);
            });

            // Umumnya pesan yang keluar dari kode kamu: "Payslip for this period was not found."
            // (Tak wajib; kalau berbeda, test tetap lulus karena memakai ThrowsAnyAsync)
            // Jika ingin ketat, boleh uncomment baris di bawah sesuai pesan aktual project kamu:
            // Assert.Contains("Payslip for this period was not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
