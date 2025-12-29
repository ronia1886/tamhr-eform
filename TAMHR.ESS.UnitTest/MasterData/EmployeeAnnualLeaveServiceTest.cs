using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class EmployeeAnnualLeaveServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;

        private readonly Mock<IRepository<EmployeeAnnualLeave>> _annualRepo;
        private readonly Mock<IRepository<User>> _userRepo;

        private readonly EmployeeAnnualLeaveService _service;

        public EmployeeAnnualLeaveServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();

            _annualRepo = new Mock<IRepository<EmployeeAnnualLeave>>();
            _userRepo = new Mock<IRepository<User>>();

            // Wiring repository yang dipakai service
            _uow.Setup(u => u.GetRepository<EmployeeAnnualLeave>()).Returns(_annualRepo.Object);
            _uow.Setup(u => u.GetRepository<User>()).Returns(_userRepo.Object);

            // SaveChanges -> sukses
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new EmployeeAnnualLeaveService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repo_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new EmployeeAnnualLeave
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                Period = 2025,
                AnnualLeave = 12,
                RowStatus = true
            };

            // Act
            _service.Upsert(model);

            // Assert
            _annualRepo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Delete_Should_Call_Repo_DeleteById_And_SaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            _service.Delete(id);

            // Assert
            _annualRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetByNoreg_WithPeriod_Should_Return_Matching_Item()
        {
            // Arrange
            var list = new List<EmployeeAnnualLeave>
            {
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP001", Period = 2025, AnnualLeave = 12, RowStatus = true },
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP001", Period = 2024, AnnualLeave = 10, RowStatus = true },
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP002", Period = 2025, AnnualLeave = 8,  RowStatus = true }
            }.AsQueryable();

            _annualRepo.Setup(r => r.Fetch()).Returns(list);

            // Act
            var result = _service.GetByNoreg("EMP001", 2025);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EMP001", result.NoReg);
            Assert.Equal(2025, result.Period);
        }

        [Fact]
        public void GetByNoreg_Should_Filter_By_NoReg()
        {
            // Arrange
            var list = new List<EmployeeAnnualLeave>
            {
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP001", Period = 2025, AnnualLeave = 12, RowStatus = true },
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP001", Period = 2024, AnnualLeave = 10, RowStatus = true },
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP002", Period = 2025, AnnualLeave = 8,  RowStatus = true }
            }.AsQueryable();

            _annualRepo.Setup(r => r.Fetch()).Returns(list);

            // Act
            var result = _service.GetByNoreg("EMP001").ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal("EMP001", x.NoReg));
        }

        [Fact]
        public void GetQuery_Should_Join_User_And_Map_Name()
        {
            // Arrange
            var leaves = new List<EmployeeAnnualLeave>
            {
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP001", Period = 2025, AnnualLeave = 12, RowStatus = true },
                new EmployeeAnnualLeave { Id = Guid.NewGuid(), NoReg = "EMP002", Period = 2025, AnnualLeave = 8,  RowStatus = true }
            }.AsQueryable();

            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), NoReg = "EMP001", Name = "Alice", RowStatus = true },
                new User { Id = Guid.NewGuid(), NoReg = "EMP002", Name = "Bob",   RowStatus = true },
                new User { Id = Guid.NewGuid(), NoReg = "EMP999", Name = "Ghost", RowStatus = true },
            }.AsQueryable();

            _annualRepo.Setup(r => r.Fetch()).Returns(leaves);
            _userRepo.Setup(r => r.Fetch()).Returns(users);

            // Act
            var query = _service.GetQuery(); // sudah AsEnumerable().AsQueryable()
            var result = query.ToList();

            // Assert
            Assert.Equal(2, result.Count);
            var a = result.Single(x => x.NoReg == "EMP001");
            var b = result.Single(x => x.NoReg == "EMP002");
            Assert.Equal("Alice", a.Name);
            Assert.Equal("Bob", b.Name);
        }
    }
}
