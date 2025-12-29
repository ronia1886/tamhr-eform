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
    public class VehicleServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Vehicle>> _vehicleRepo;
        private readonly Mock<IRepository<VehicleMatrix>> _vehicleMatrixRepo;
        private readonly VehicleService _service;

        public VehicleServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _vehicleRepo = new Mock<IRepository<Vehicle>>();
            _vehicleMatrixRepo = new Mock<IRepository<VehicleMatrix>>();

            // Repo yang dipakai oleh GenericDomainServiceBase<Vehicle>
            _uow.Setup(u => u.GetRepository<Vehicle>()).Returns(_vehicleRepo.Object);
            // Diset juga untuk jaga-jaga jika dipakai pada test lain
            _uow.Setup(u => u.GetRepository<VehicleMatrix>()).Returns(_vehicleMatrixRepo.Object);

            // SaveChanges -> return 1 (sukses)
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new VehicleService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repository_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new Vehicle { Id = Guid.NewGuid() };

            // Properties yang di-update sesuai VehicleService.Properties
            var expectedProps = new[]
            {
                "TypeName",
                "Type",
                "ModelCode",
                "COP",
                "CPP",
                "SCP",
                "Suffix",
                "FinalPrice",
                "Colors"
            };

            // Act
            _service.Upsert(model);

            // Assert
            _vehicleRepo.Verify(r => r.Upsert<Guid>(
                    It.Is<Vehicle>(v => v == model),
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
            _vehicleRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
