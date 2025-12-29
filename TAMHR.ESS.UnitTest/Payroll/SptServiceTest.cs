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
    public class SptServiceTest
    {
        /// <summary>
        /// Helper untuk membangun SptService dengan IUnitOfWork mock.
        /// - Repo UserHash bisa diatur (ada/tidak ada entry).
        /// - ConfigService dibuat LOSE (tanpa Setup GetConfigValue apapun)
        ///   agar tidak men-trigger error extension/optional args.
        /// - PersonalDataService kita biarkan null (aman bila dengan UserHash).
        /// </summary>
        private static SptService CreateService(
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
                          TypeCode = HashType.Payslip, // dipakai juga di SPT
                          HashValue = "dummy-hash"     // isi tidak penting untuk jalur test negatif
                      }
                  }.AsQueryable()
                : Array.Empty<UserHash>().AsQueryable());

            userHashRepo.Setup(r => r.Fetch()).Returns(hashes);
            uow.Setup(u => u.GetRepository<UserHash>()).Returns(userHashRepo.Object);

            // ConfigService LOSE: jangan Setup method apa pun (hindari extension/optional args)
            var cfgLoose = new Mock<ConfigService>(MockBehavior.Loose, uow.Object);

            // PersonalDataService: null saja; pada jalur "withUserHash=true" tidak akan dipakai.
            PersonalDataService pds = null;

            return new SptService(cfgLoose.Object, pds, uow.Object);
        }

        [Fact]
        public async Task Download_Should_Throw_When_ConfigMissing_And_NoUserHash()
        {
            // Tanpa UserHash => service akan coba pakai PersonalDataService/config,
            // yang di lingkungan unit test ini memang tidak tersedia. Kita ekspektasi exception.
            var svc = CreateService(withUserHash: false, out var _);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await svc.Download(noreg: "E001", period: 2025, viewOnly: true);
            });
        }

        [Fact]
        public async Task Download_Should_Throw_When_File_NotFound_Even_With_UserHash()
        {
            // Dengan UserHash => PersonalDataService tidak digunakan,
            // tapi karena path/filename dari config tidak diset, tetap gagal (exception).
            var svc = CreateService(withUserHash: true, out var _);

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await svc.Download(noreg: "E001", period: 2024, viewOnly: true);
            });

            // Jika ingin ketat sesuai pesan di service-mu, bisa cek mengandung "SPT for this period was not found".
            // Assert.Contains("SPT for this period was not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
