using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.TimeManagement;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class EmployeeLeaveServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();

        // Repos yang dipakai service
        private readonly Mock<IRepository<EmployeeLeave>> _repoEmployeeLeave = new Mock<IRepository<EmployeeLeave>>();
        private readonly Mock<IRepository<ActualOrganizationStructure>> _repoActualOrg = new Mock<IRepository<ActualOrganizationStructure>>();
        private readonly Mock<IRepository<EmployeeLeaveHistoryView>> _repoLeaveHistory = new Mock<IRepository<EmployeeLeaveHistoryView>>();
        private readonly Mock<IRepository<EmployeeManualAbsent>> _repoManualAbsent = new Mock<IRepository<EmployeeManualAbsent>>();
        private readonly Mock<IRepository<User>> _repoUser = new Mock<IRepository<User>>();
        private readonly Mock<IRepository<WorkSchedule>> _repoWorkSchedule = new Mock<IRepository<WorkSchedule>>();

        private readonly EmployeeLeaveService _service;

        // In-memory data
        private readonly List<EmployeeLeave> _employeeLeaves = new List<EmployeeLeave>();
        private readonly List<ActualOrganizationStructure> _actualOrgs = new List<ActualOrganizationStructure>();
        private readonly List<EmployeeLeaveHistoryView> _histories = new List<EmployeeLeaveHistoryView>();
        private readonly List<EmployeeManualAbsent> _manuals = new List<EmployeeManualAbsent>();
        private readonly List<User> _users = new List<User>();
        private readonly List<WorkSchedule> _workSchedules = new List<WorkSchedule>();

        public EmployeeLeaveServiceTest()
        {
            // Seed data
            _employeeLeaves.Add(new EmployeeLeave
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                AnnualLeave = 12,
                LongLeave = 3
            });

            _actualOrgs.Add(new ActualOrganizationStructure
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                Name = "Alice",
                Staffing = 100
            });

            _histories.Add(new EmployeeLeaveHistoryView
            {
                noreg = "EMP001",
                Period = DateTime.Now.Year,
                leavetype = "annual-leave",
                UsedLeave = 10
            });
            _histories.Add(new EmployeeLeaveHistoryView
            {
                noreg = "EMP001",
                Period = DateTime.Now.Year,
                leavetype = "long-leave",
                UsedLeave = 6
            });

            _manuals.Add(new EmployeeManualAbsent
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                LeaveType = "annual-Leave",
                Period = new DateTime(DateTime.Now.Year, 1, 1),
                RowStatus = true,
                UsedEdited = 3
            });
            _manuals.Add(new EmployeeManualAbsent
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                LeaveType = "Long-Leave",
                Period = new DateTime(DateTime.Now.Year, 1, 1),
                RowStatus = true,
                UsedEdited = 2
            });

            _users.Add(new User { Id = Guid.NewGuid(), NoReg = "EMP001", Name = "Alice" });

            // Wire GetRepository<>
            _uow.Setup(u => u.GetRepository<EmployeeLeave>()).Returns(_repoEmployeeLeave.Object);
            _uow.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_repoActualOrg.Object);
            _uow.Setup(u => u.GetRepository<EmployeeLeaveHistoryView>()).Returns(_repoLeaveHistory.Object);
            _uow.Setup(u => u.GetRepository<EmployeeManualAbsent>()).Returns(_repoManualAbsent.Object);
            _uow.Setup(u => u.GetRepository<User>()).Returns(_repoUser.Object);
            _uow.Setup(u => u.GetRepository<WorkSchedule>()).Returns(_repoWorkSchedule.Object);

            // IQueryable dari list
            _repoEmployeeLeave.Setup(r => r.Fetch()).Returns(_employeeLeaves.AsQueryable());
            _repoActualOrg.Setup(r => r.Fetch()).Returns(_actualOrgs.AsQueryable());
            _repoLeaveHistory.Setup(r => r.Fetch()).Returns(_histories.AsQueryable());
            _repoManualAbsent.Setup(r => r.Fetch()).Returns(_manuals.AsQueryable());
            _repoUser.Setup(r => r.Fetch()).Returns(_users.AsQueryable());
            _repoWorkSchedule.Setup(r => r.Fetch()).Returns(_workSchedules.AsQueryable());

            // SaveChanges default
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            // PENTING: Jangan jalankan action Transact agar extension UspQuery tidak terpanggil
            _uow.Setup(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()))
                .Callback<Action<IDbTransaction>, IsolationLevel>((act, _) =>
                {
                    // sengaja DIBIARKAN kosong — JANGAN panggil act(...)
                });

            _service = new EmployeeLeaveService(_uow.Object);
        }

        [Fact]
        public void GetQuery_Should_Join_With_ActualOrganization()
        {
            var list = _service.GetQuery().ToList();
            Assert.NotEmpty(list);
            var item = list.First();
            Assert.Equal("EMP001", item.NoReg);
            Assert.Equal("Alice", item.Name);
        }

        [Fact]
        public void GetByNoreg_Should_Return_Data()
        {
            var res = _service.GetByNoreg("EMP001");
            Assert.NotNull(res);
            Assert.Equal("EMP001", res.NoReg);
            Assert.Equal(12, res.AnnualLeave);
            Assert.Equal(3, res.LongLeave);
        }

        [Fact]
        public void GetByNoregCurrentYear_Should_Return_From_EmployeeLeaveRepo()
        {
            var res = _service.GetByNoregCurrentYear("EMP001");
            Assert.NotNull(res);
            Assert.Equal(12, res.AnnualLeave);
            Assert.Equal(3, res.LongLeave);
        }

        [Fact]
        public void GetByNoregAndYear_Should_Return_From_EmployeeLeaveRepo()
        {
            var res = _service.GetByNoregAndYear("EMP001", DateTime.Now.Year);
            Assert.NotNull(res);
            Assert.Equal(12, res.AnnualLeave);
            Assert.Equal(3, res.LongLeave);
        }

        [Fact]
        public void ValidateWorkSchedule_Should_NotThrow_When_All_Dates_Present()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 3);
            _workSchedules.Clear();
            _workSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), Date = new DateTime(2025, 1, 1) });
            _workSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), Date = new DateTime(2025, 1, 2) });
            _workSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), Date = new DateTime(2025, 1, 3) });

            var ex = Record.Exception(() => _service.ValidateWorkSchedule("EMP001", start, end));
            Assert.Null(ex);
        }

        [Fact]
        public void ValidateWorkSchedule_Should_Throw_When_Missing_Date()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 3);
            _workSchedules.Clear();
            _workSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), Date = new DateTime(2025, 1, 1) });
            _workSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), Date = new DateTime(2025, 1, 3) });

            Assert.ThrowsAny<Exception>(() => _service.ValidateWorkSchedule("EMP001", start, end));
        }

        [Fact]
        public void GetEmployeeLeaveHistory_Should_Return_List()
        {
            var list = _service.GetEmployeeLeaveHistory("EMP001");
            Assert.NotNull(list);
            Assert.True(list.Count >= 2);
        }

        [Fact]
        public void GetUsedAnnualLeaveWithoutManual_Should_Subtract_Manual()
        {
            var period = DateTime.Now.Year.ToString();
            var result = _service.GetUsedAnnualLeaveWithoutManual("EMP001", period);
            // 10 (history) - 3 (manual) = 7
            Assert.Equal(7, result);
        }

        [Fact]
        public void GetUsedLongLeaveWithoutManual_Should_Subtract_Manual()
        {
            var period = DateTime.Now.Year.ToString();
            var result = _service.GetUsedLongLeaveWithoutManual("EMP001", period);
            // 6 (history) - 2 (manual) = 4
            Assert.Equal(4, result);
        }

        [Fact]
        public void UpdateLeave_Should_Return_True_And_Call_Transact()
        {
            var vm = new EmployeeLeaveViewModel
            {
                noreg = "EMP001",
                Period = DateTime.Now.Year.ToString(),
                TotalLeave = 12,
                UsedLeave = 5,
                RemainingLeave = 7,
                ModifiedBy = "EMP001"
            };

            var ok = _service.UpdateLeave(vm);

            Assert.True(ok);
            _uow.Verify(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()), Times.Once);
            // JANGAN verify UspQuery atau SaveChanges di sini, karena action Transact tidak dijalankan
        }

        [Fact]
        public void UpdateLongLeave_Should_Return_True_And_Call_Transact()
        {
            var vm = new EmployeeLeaveViewModel
            {
                noreg = "EMP001",
                Period = DateTime.Now.Year.ToString(),
                TotalLeave = 0,
                UsedLeave = 2,
                RemainingLeave = 1,
                ModifiedBy = "EMP001"
            };

            var ok = _service.UpdateLongLeave(vm);

            Assert.True(ok);
            _uow.Verify(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()), Times.Once);
        }

        [Fact]
        public void AddLeaveEmployee_Should_Return_True_And_Call_Transact()
        {
            var vm = new AddEmployeeLeaveViewModel
            {
                noreg = "EMP001",
                Period = DateTime.Now.Year.ToString(),
                TotalLeave = 12,
                UsedLeave = 3,
                RemainingLeave = 9,
                ModifiedBy = "EMP001",
                TotalLongLeave = 6,
                UsedLongLeave = 1,
                RemainingLongLeave = 5,
                PeriodLongLeave = DateTime.Now.Year.ToString()
            };

            var ok = _service.AddLeaveEmployee(vm);

            Assert.True(ok);
            _uow.Verify(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()), Times.Once);
        }

        [Fact]
        public void ProcessUploadAsync_Should_Throw_When_File_Null()
        {
            Assert.ThrowsAnyAsync<Exception>(() => _service.ProcessUploadAsync(null, "EMP001"));
        }
    }
}
