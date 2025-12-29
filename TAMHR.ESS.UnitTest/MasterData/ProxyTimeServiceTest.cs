using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class ProxyTimeServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<TimeManagement>> _timeManagementRepo;
        private readonly Mock<IRepository<TimeManagementHistory>> _timeManagementHistoryRepo;
        private readonly Mock<IRepository<EmpWorkSchSubtitute>> _empWorkSchSubtituteRepo;
        private readonly Mock<IRepository<DailyWorkSchedule>> _dailyWorkScheduleRepo;

        private readonly ProxyTimeService _service;

        public ProxyTimeServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _timeManagementRepo = new Mock<IRepository<TimeManagement>>();
            _timeManagementHistoryRepo = new Mock<IRepository<TimeManagementHistory>>();
            _empWorkSchSubtituteRepo = new Mock<IRepository<EmpWorkSchSubtitute>>();
            _dailyWorkScheduleRepo = new Mock<IRepository<DailyWorkSchedule>>();

            // default setup Transact agar langsung invoke action
            _unitOfWork
                .Setup(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()))
                .Callback<Action<IDbTransaction>, IsolationLevel>((a, _) => a(null));

            // setup repositori default
            _unitOfWork.Setup(u => u.GetRepository<TimeManagement>()).Returns(_timeManagementRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<TimeManagementHistory>()).Returns(_timeManagementHistoryRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmpWorkSchSubtitute>()).Returns(_empWorkSchSubtituteRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DailyWorkSchedule>()).Returns((IRepository<DailyWorkSchedule>)null!);

            // readonly repo
            _unitOfWork.Setup(u => u.GetRepository<DailyWorkSchedule>()).Returns(_dailyWorkScheduleRepo.Object);

            // inject ke service
            _service = new ProxyTimeService(_unitOfWork.Object);
        }

        [Fact]
        public void Upsert_ShouldCallUpsertAndSaveChanges()
        {
            // Arrange
            var model = new TimeManagement { Id = Guid.NewGuid(), RowStatus = true, ShiftCode = "S1" };
            var daily = new DailyWorkSchedule { Id = Guid.NewGuid(), ShiftCode = "S1" };

            _timeManagementRepo.Setup(r => r.Fetch()).Returns(new List<TimeManagement> { model }.AsQueryable());
            _dailyWorkScheduleRepo.Setup(r => r.Fetch()).Returns(new List<DailyWorkSchedule> { daily }.AsQueryable());

            // Act
            _service.Upsert("EMP001", model);

            // Assert
            _timeManagementRepo.Verify(r => r.Upsert<Guid>(It.IsAny<TimeManagement>(), It.IsAny<string[]>()), Times.Once);
            _timeManagementHistoryRepo.Verify(r => r.Add(It.IsAny<TimeManagementHistory>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        public void Update_ShouldCallUpsertAndSaveChanges()
        {
            // Arrange
            var model = new TimeManagement { Id = Guid.NewGuid(), RowStatus = true, ShiftCode = "S1" };
            var daily = new DailyWorkSchedule { Id = Guid.NewGuid(), ShiftCode = "S1" };

            _timeManagementRepo.Setup(r => r.Fetch()).Returns(new List<TimeManagement> { model }.AsQueryable());
            _dailyWorkScheduleRepo.Setup(r => r.Fetch()).Returns(new List<DailyWorkSchedule> { daily }.AsQueryable());

            // Act
            _service.Upsert("EMP001", model);

            // Assert
            _timeManagementRepo.Verify(r => r.Upsert<Guid>(It.IsAny<TimeManagement>(), It.IsAny<string[]>()), Times.Once);
            _timeManagementHistoryRepo.Verify(r => r.Add(It.IsAny<TimeManagementHistory>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteHistory()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            _service.DeleteHistory(id);

            // Assert
            _timeManagementHistoryRepo.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
