using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class VaccineScheduleServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;

        private readonly Mock<IRepository<VaccineSchedule>> _scheduleRepo;
        private readonly Mock<IRepository<VaccineScheduleLimit>> _limitRepo;
        private readonly Mock<IRepository<VaccineHospital>> _hospitalRepo;
        private readonly Mock<IRepository<VaccineScheduleLimitStoredEntity>> _storedRepo;

        private readonly VaccineScheduleService _service;

        public VaccineScheduleServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();

            _scheduleRepo = new Mock<IRepository<VaccineSchedule>>();
            _limitRepo = new Mock<IRepository<VaccineScheduleLimit>>();
            _hospitalRepo = new Mock<IRepository<VaccineHospital>>();
            _storedRepo = new Mock<IRepository<VaccineScheduleLimitStoredEntity>>();

            // Wiring repository yg dipakai service
            _uow.Setup(u => u.GetRepository<VaccineSchedule>()).Returns(_scheduleRepo.Object);
            _uow.Setup(u => u.GetRepository<VaccineScheduleLimit>()).Returns(_limitRepo.Object);
            _uow.Setup(u => u.GetRepository<VaccineHospital>()).Returns(_hospitalRepo.Object);
            _uow.Setup(u => u.GetRepository<VaccineScheduleLimitStoredEntity>()).Returns(_storedRepo.Object);

            // SaveChanges mengembalikan > 0 agar method boolean (UpsertDetail/DeleteDetailById) -> true
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            // (Opsional) kalau nanti dipakai, Transact langsung invoke action
            _uow
                .Setup(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()))
                .Callback<Action<IDbTransaction>, IsolationLevel>((a, _) => a(null));

            _service = new VaccineScheduleService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repository_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new VaccineSchedule
            {
                Id = Guid.NewGuid(),
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddHours(2),
                VaccineNumber = 1,
                RowStatus = true
            };

            // Act
            _service.Upsert(model);

            // Assert
            _scheduleRepo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
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
            _scheduleRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpsertDetail_Should_Call_Repo_Upsert_And_SaveChanges_Return_True()
        {
            // Arrange
            var limit = new VaccineScheduleLimit
            {
                Id = Guid.NewGuid(),
                VaccineScheduleId = Guid.NewGuid(),
                VaccineHospitalId = Guid.NewGuid(),
                VaccineDate = DateTime.Today,
                Qty = 50,
                RowStatus = true
            };

            // Act
            var result = _service.UpsertDetail(limit);

            // Assert
            Assert.True(result);
            _limitRepo.Verify(r => r.Upsert<Guid>(limit, It.IsAny<string[]>()), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteDetailById_Should_Call_Repo_DeleteById_And_SaveChanges_Return_True()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = _service.DeleteDetailById(id);

            // Assert
            Assert.True(result);
            _limitRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetVaccineSchedule_Should_Filter_By_Id()
        {
            // Arrange
            var keepId = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            var data = new List<VaccineSchedule>
            {
                new VaccineSchedule { Id = keepId, RowStatus = true },
                new VaccineSchedule { Id = otherId, RowStatus = true }
            }.AsQueryable();

            _scheduleRepo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetVaccineSchedule(keepId).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(keepId, result[0].Id);
        }

        [Fact]
        public void GetVaccineHospitals_Should_Return_Only_RowStatus_True()
        {
            // Arrange
            var h1 = new VaccineHospital { Id = Guid.NewGuid(), Name = "RS A", RowStatus = true };
            var h2 = new VaccineHospital { Id = Guid.NewGuid(), Name = "RS B", RowStatus = false };

            _hospitalRepo.Setup(r => r.Fetch()).Returns(new List<VaccineHospital> { h1, h2 }.AsQueryable());

            // Act
            var result = _service.GetVaccineHospitals().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("RS A", result[0].Name);
        }

        [Fact]
        public void GetVaccineScheduleLimit_Should_Filter_By_ScheduleId()
        {
            // Arrange
            var scheduleId = Guid.NewGuid();
            var otherScheduleId = Guid.NewGuid();

            var items = new List<VaccineScheduleLimitStoredEntity>
            {
                new VaccineScheduleLimitStoredEntity { VaccineScheduleId = scheduleId },
                new VaccineScheduleLimitStoredEntity { VaccineScheduleId = otherScheduleId },
            }.AsQueryable();

            _storedRepo.Setup(r => r.Fetch()).Returns(items);

            // Act
            var result = _service.GetVaccineScheduleLimit(scheduleId).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(scheduleId, result[0].VaccineScheduleId);
        }

        [Fact]
        public void GetVaccineScheduleLimitById_Should_Return_Matching_And_RowStatus_True()
        {
            // Arrange
            var idKeep = Guid.NewGuid();
            var idOther = Guid.NewGuid();

            var data = new List<VaccineScheduleLimit>
            {
                new VaccineScheduleLimit { Id = idKeep, RowStatus = true },
                new VaccineScheduleLimit { Id = idOther, RowStatus = true },
                new VaccineScheduleLimit { Id = idKeep, RowStatus = false }, // same Id but RowStatus false -> harusnya terfilter
            }.AsQueryable();

            _limitRepo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetVaccineScheduleLimitById(idKeep);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(idKeep, result.Id);
            Assert.True(result.RowStatus);
        }

        [Fact]
        public void GetVaccineScheduleByDateHospitalName_Should_Return_Matching_Limit()
        {
            // Arrange
            var date = new DateTime(2025, 1, 10);
            var hospitalName = "RS ABC";
            var hospId = Guid.NewGuid();

            var hospitals = new List<VaccineHospital>
            {
                new VaccineHospital { Id = hospId, Name = hospitalName, RowStatus = true },
                new VaccineHospital { Id = Guid.NewGuid(), Name = "RS XYZ", RowStatus = true }
            }.AsQueryable();

            var limits = new List<VaccineScheduleLimit>
            {
                new VaccineScheduleLimit { Id = Guid.NewGuid(), VaccineHospitalId = hospId, VaccineDate = date, Qty = 10, RowStatus = true },
                new VaccineScheduleLimit { Id = Guid.NewGuid(), VaccineHospitalId = hospId, VaccineDate = date.AddDays(1), Qty = 5, RowStatus = true }
            }.AsQueryable();

            _hospitalRepo.Setup(r => r.Fetch()).Returns(hospitals);
            _limitRepo.Setup(r => r.Fetch()).Returns(limits);

            // Act
            var result = _service.GetVaccineScheduleByDateHospitalName(date, hospitalName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(hospId, result.VaccineHospitalId);
            Assert.Equal(date, result.VaccineDate);
        }

        [Fact]
        public void GetVaccineScheduleByDate_Should_Join_Hospitals_With_Limits_On_Date()
        {
            // Arrange
            var date = new DateTime(2025, 1, 20);

            var h1 = new VaccineHospital { Id = Guid.NewGuid(), Name = "RS 1", RowStatus = true };
            var h2 = new VaccineHospital { Id = Guid.NewGuid(), Name = "RS 2", RowStatus = true };
            var h3 = new VaccineHospital { Id = Guid.NewGuid(), Name = "RS 3", RowStatus = true };

            var limits = new List<VaccineScheduleLimit>
            {
                new VaccineScheduleLimit { Id = Guid.NewGuid(), VaccineHospitalId = h1.Id, VaccineDate = date, Qty = 10, RowStatus = true },
                new VaccineScheduleLimit { Id = Guid.NewGuid(), VaccineHospitalId = h2.Id, VaccineDate = date, Qty = 5, RowStatus = true },
                // h3 tidak punya limit utk date tersebut -> tidak ikut hasil join
            }.AsQueryable();

            _hospitalRepo.Setup(r => r.Fetch()).Returns(new List<VaccineHospital> { h1, h2, h3 }.AsQueryable());
            _limitRepo.Setup(r => r.Fetch()).Returns(limits);

            // Act
            var result = _service.GetVaccineScheduleByDate(date).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // hanya RS 1 & RS 2
            Assert.Contains(result, v => v.Id == h1.Id);
            Assert.Contains(result, v => v.Id == h2.Id);
            Assert.DoesNotContain(result, v => v.Id == h3.Id);
        }
    }
}
