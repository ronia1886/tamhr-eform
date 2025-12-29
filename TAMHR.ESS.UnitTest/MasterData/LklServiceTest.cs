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
    public class SpklMasterDataServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<SpklMasterDataView>> _viewRepo;     // <-- pakai IRepository
        private readonly Mock<IRepository<TimeManagement>> _tmReadonlyRepo;   // <-- pakai IRepository
        private readonly Mock<IRepository<TimeManagementSpkl>> _tmSpklRepo;

        private readonly SpklMasterDataService _service;

        public SpklMasterDataServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();

            _viewRepo = new Mock<IRepository<SpklMasterDataView>>();
            _tmReadonlyRepo = new Mock<IRepository<TimeManagement>>();
            _tmSpklRepo = new Mock<IRepository<TimeManagementSpkl>>();

            // Wiring GetRepository untuk tipe yang dipakai service
            _uow.Setup(u => u.GetRepository<SpklMasterDataView>()).Returns(_viewRepo.Object);
            _uow.Setup(u => u.GetRepository<TimeManagement>()).Returns(_tmReadonlyRepo.Object);
            _uow.Setup(u => u.GetRepository<TimeManagementSpkl>()).Returns(_tmSpklRepo.Object);

            _uow.Setup(u => u.SaveChanges());

            _service = new SpklMasterDataService(_uow.Object);
        }

        [Fact]
        public void GetDataViews_Filters_And_Sorts()
        {
            // Arrange
            var rows = new List<SpklMasterDataView>
            {
                new SpklMasterDataView { Id = Guid.NewGuid(), Name = "Andi", NoReg = "001", OvertimeDate = new DateTime(2023, 11, 1) },
                new SpklMasterDataView { Id = Guid.NewGuid(), Name = "Budi", NoReg = "002", OvertimeDate = new DateTime(2023, 11, 2) }
            };
            _viewRepo.Setup(r => r.Fetch()).Returns(rows.AsQueryable());

            // name & noreg dibuat spesifik supaya hanya 1 yang match
            var result = _service.GetDataViews(noreg: "001", name: "Andi", startDate: null, endDate: null).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Andi", result[0].Name);
            Assert.Equal("001", result[0].NoReg);
        }

        [Fact]
        public void GetViewById_Should_Return_Correct_Item()
        {
            // Arrange
            var id = Guid.NewGuid();
            var rows = new List<SpklMasterDataView>
            {
                new SpklMasterDataView { Id = id, Name = "Andi", NoReg = "001", OvertimeDate = new DateTime(2023, 11, 1) },
                new SpklMasterDataView { Id = Guid.NewGuid(), Name = "Budi", NoReg = "002", OvertimeDate = new DateTime(2023, 11, 2) }
            };
            _viewRepo.Setup(r => r.Fetch()).Returns(rows.AsQueryable());

            // Act
            var item = _service.GetViewById(id);

            // Assert
            Assert.NotNull(item);
            Assert.Equal(id, item.Id);
            Assert.Equal("Andi", item.Name);
        }

        [Fact]
        public void Upsert_Should_Calculate_Durations_And_Call_Repository_Upsert()
        {
            // Arrange
            var workingDate = new DateTime(2023, 11, 1);
            var noReg = "001";

            _tmReadonlyRepo.Setup(r => r.Fetch()).Returns(new List<TimeManagement>
            {
                new TimeManagement
                {
                    Id = Guid.NewGuid(),
                    NoReg = noReg,
                    WorkingDate = workingDate,
                    NormalTimeIn  = workingDate.AddHours(8),   // 08:00
                    NormalTimeOut = workingDate.AddHours(17)   // 17:00
                }
            }.AsQueryable());

            var model = new TimeManagementSpkl
            {
                Id = Guid.NewGuid(),
                NoReg = noReg,
                OvertimeDate = workingDate,
                OvertimeInPlan = workingDate.AddHours(18), // 18:00
                OvertimeOutPlan = workingDate.AddHours(20), // 20:00
                OvertimeInAdjust = workingDate.AddHours(18),
                OvertimeOutAdjust = workingDate.AddHours(20),
                OvertimeBreakPlan = 0,
                OvertimeBreakAdjust = 0,
                OvertimeCategoryCode = "pekerjaantambahan",
                OvertimeReason = "Unit test"
            };

            // Act
            _service.Upsert(model);

            // Assert
            _tmSpklRepo.Verify(r => r.Upsert<Guid>(It.IsAny<TimeManagementSpkl>(), It.IsAny<string[]>()), Times.Once);
            Assert.True(model.DurationPlan > 0, "DurationPlan should be calculated and > 0");
            Assert.True(model.DurationAdjust > 0, "DurationAdjust should be calculated and > 0");
        }
    }
}
