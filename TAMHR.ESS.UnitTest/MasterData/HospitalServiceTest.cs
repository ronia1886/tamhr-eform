using System;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class HospitalServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Hospital>> _repo;
        private readonly HospitalService _service;

        public HospitalServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IRepository<Hospital>>();

            // GenericDomainServiceBase<T> akan memanggil GetRepository<Hospital>()
            _uow.Setup(u => u.GetRepository<Hospital>())
                .Returns(_repo.Object);

            // Kita hanya ingin verifikasi dipanggil
            _uow.Setup(u => u.SaveChanges());

            _service = new HospitalService(_uow.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new Hospital
            {
                Id = Guid.NewGuid()
                // Boleh isi properti lain bila ada: Name, Province, City, Address
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
