using System;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class BankServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Bank>> _bankRepo;
        private readonly BankService _service;

        public BankServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _bankRepo = new Mock<IRepository<Bank>>();

            // Repo utama yang dipakai GenericDomainServiceBase<Bank>
            _uow.Setup(u => u.GetRepository<Bank>()).Returns(_bankRepo.Object);

            // SaveChanges sukses
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new BankService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repository_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new Bank { Id = Guid.NewGuid() };

            var expectedProps = new[]
            {
                "BankKey",
                "BankName",
                "Branch",
                "City",
                "Address"
            };

            // Act
            _service.Upsert(model);

            // Assert
            _bankRepo.Verify(r => r.Upsert<Guid>(
                    It.Is<Bank>(b => b == model),
                    It.Is<string[]>(props => props.SequenceEqual(expectedProps))
                ),
                Times.Once);

            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Should_Call_Repository_DeleteById_And_SaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            _service.DeleteById(id);

            // Assert
            _bankRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
