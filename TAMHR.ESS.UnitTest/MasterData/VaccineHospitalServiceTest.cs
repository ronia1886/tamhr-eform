using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class VaccineHospitalServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<VaccineHospital>> _repo;
        private readonly VaccineHospitalService _service;

        public VaccineHospitalServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IRepository<VaccineHospital>>();

            // wiring repository VaccineHospital yang dipakai GenericDomainServiceBase<VaccineHospital>
            _uow.Setup(u => u.GetRepository<VaccineHospital>()).Returns(_repo.Object);
            _uow.Setup(u => u.SaveChanges());

            _service = new VaccineHospitalService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repository_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new VaccineHospital
            {
                Id = Guid.NewGuid(),
                Name = "RS Contoh",
                Province = "DKI Jakarta",
                City = "Jakarta Selatan",
                Address = "Jl. Contoh No. 1",
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

        // Catatan: Method GetHospitalByName dan GetScheduleByHospitalDate di service
        // memakai Dapper (UnitOfWork.GetConnection().Query<T>).
        // Untuk unit test murni tanpa DB, biasanya dibuat integration test atau
        // abstraksi tambahan pada lapisan data. Jadi di sini kita fokus ke operasi CRUD via repository.
    }

    public class VaccineHospitalRepoServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Hospital>> _hospitalRepo;
        private readonly VaccineHospitalRepoService _repoService;

        public VaccineHospitalRepoServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _hospitalRepo = new Mock<IRepository<Hospital>>();

            _uow.Setup(u => u.GetRepository<Hospital>()).Returns(_hospitalRepo.Object);

            _repoService = new VaccineHospitalRepoService(_uow.Object);
        }

        [Fact]
        public void GetHospital_Should_Fetch_AsNoTracking_And_Return_Data()
        {
            // Arrange
            var data = new List<Hospital>
            {
                new Hospital { Id = Guid.NewGuid(), Name = "RS A", City = "Jakarta", Province = "DKI", Address = "Jl. A" , RowStatus = true},
                new Hospital { Id = Guid.NewGuid(), Name = "RS B", City = "Bandung", Province = "Jabar", Address = "Jl. B" , RowStatus = true}
            }.AsQueryable();

            _hospitalRepo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _repoService.GetHospital().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "RS A");
            Assert.Contains(result, x => x.Name == "RS B");
            _hospitalRepo.Verify(r => r.Fetch(), Times.Once);
        }
    }
}
