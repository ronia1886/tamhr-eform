using System;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class AllowanceDetailServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<AllowanceDetail>> _repo;
        private readonly AllowanceDetailService _service;

        public AllowanceDetailServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IRepository<AllowanceDetail>>();

            // repository yang dipakai GenericDomainServiceBase<T>
            _uow.Setup(u => u.GetRepository<AllowanceDetail>()).Returns(_repo.Object);
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new AllowanceDetailService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repository_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new AllowanceDetail
            {
                Id = Guid.NewGuid(),
                Type = "Meal",
                SubType = "Shift",
                ClassFrom = 1,                 // int (bukan string)
                ClassTo = 3,                 // int (bukan string)
                Ammount = 150000m,
                Description = "Uang makan shift",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                RowStatus = true
            };

            // Act
            _service.Upsert(model);

            // Assert
            _repo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
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
            _repo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
