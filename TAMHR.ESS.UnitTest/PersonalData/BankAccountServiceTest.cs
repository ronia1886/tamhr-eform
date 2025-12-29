using System;
using Moq;
using Xunit;

using Agit.Domain.UnitOfWork;
// Ganti namespace ini jika berbeda dengan header PersonalDataService.cs
using TAMHR.ESS.Infrastructure.DomainServices;
// Entity ada di namespace ini (sesuai file yang kamu kirim)
using TAMHR.ESS.Domain;

// PENTING: pakai namespace tempat IRepository<> berada di project kamu.
// Umumnya: Agit.Domain.Repositories
// Jika tidak ada, buka IRepository.cs dan salin namespace persisnya.
using Agit.Domain.Repository;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class BankAccountServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<PersonalDataBankAccount>> _mockRepo;
        private readonly PersonalDataService _service;

        public BankAccountServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            _mockRepo = new Mock<IRepository<PersonalDataBankAccount>>(MockBehavior.Strict);

            // Ketika service minta repo PersonalDataBankAccount, kembalikan mock ini
            _mockUnitOfWork
                .Setup(u => u.GetRepository<PersonalDataBankAccount>())
                .Returns(_mockRepo.Object);

            // Service akan memanggil Upsert<Guid>(dbitem);
            _mockRepo
                .Setup(r => r.Upsert<Guid>(It.IsAny<PersonalDataBankAccount>()));

            // Service akan memanggil SaveChanges();
            _mockUnitOfWork
                .Setup(u => u.SaveChanges())
                .Returns(1);

            // Jika ctor butuh dependency lain (localizer), sesuaikan argumen ini
            _service = new PersonalDataService(_mockUnitOfWork.Object, /* localizer: */ null);
        }

        [Fact]
        public void UpsertBankAccount_ShouldNotThrow()
        {
            // Arrange
            var item = new PersonalDataBankAccount
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                AccountName = "John Doe",
                AccountNumber = "1234567890",
                BankCode = "014",
                RowStatus = true,
                StartDate = DateTime.Today
            };

            // Act
            var ex = Record.Exception(() => _service.UpsertPersonalDataBankAccount(item));

            // Assert
            Assert.Null(ex);

            // Optional: verifikasi interaksi
            _mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<PersonalDataBankAccount>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
