using System;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class FaskesServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Faskes>> _repo;
        private readonly FaskesService _service;

        public FaskesServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IRepository<Faskes>>();

            // GenericDomainServiceBase<T> akan memanggil GetRepository<Faskes>()
            _uow.Setup(u => u.GetRepository<Faskes>())
                .Returns(_repo.Object);

            // Kita hanya ingin verifikasi dipanggil; nilai return default OK
            _uow.Setup(u => u.SaveChanges());

            _service = new FaskesService(_uow.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new Faskes
            {
                Id = Guid.NewGuid(),
                FaskesCode = "FK-01",
                FaskesName = "Klinik Pratama",
                FaskesAddress = "Jl. Mawar No. 1",
                FaskesCity = "Jakarta",
                RowStatus = true
            };

            // Act
            _service.Upsert(model);

            // Assert
            _repo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ShouldCallRepositoryDeleteById_AndSaveChanges()
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
