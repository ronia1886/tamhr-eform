using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;
using Moq;
using TAMHR.ESS.WebUI.Areas.OHS.Controllers;
//using TAMHR.ESS.Infrastructure.DomainServices.Models;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using TAMHR.ESS.Infrastructure.Web;
using Microsoft.AspNetCore.Http;
using Xunit;
using Kendo.Mvc.UI;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Newtonsoft.Json.Linq;
using System.Linq;
using TAMHR.ESS.Infrastructure.Modules.OHS; // contoh, sesuaikan dengan project kamu
using TAMHR.ESS.Infrastructure.Extensions;


namespace TAMHR.ESS.UnitTest.OHS
{
    public class AreaActivityControllerTests
    {
        //private readonly AreaActivityApiController _controller;
        private readonly Mock<ITotalEmployeeService> _mockTotalEmployeeService;
        private readonly Mock<IServiceProxy> _mockServiceProxy;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<TotalEmployeeModel>> _mockRepo;
        private readonly Mock<IRepository<TotalEmployeeViewModel>> _mockRepoView;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ILogger<TotalEmployeeService>> _mockLogger;
        private readonly Mock<TotalEmployeeService> _totalEmployeeService;

        public AreaActivityControllerTests()
        {
            _mockTotalEmployeeService = new Mock<ITotalEmployeeService>();
            _mockServiceProxy = new Mock<IServiceProxy>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUow = new Mock<IUnitOfWork>();

            _mockServiceProxy.Setup(x => x.GetService<ITotalEmployeeService>())
                             .Returns(_mockTotalEmployeeService.Object);

            var userClaim = new UserClaim { NoReg = "TEST123" };
            _mockServiceProxy.Setup(x => x.UserClaim).Returns(userClaim);

            _mockRepo = new Mock<IRepository<TotalEmployeeModel>>();

            _mockUnitOfWork.Setup(u => u.GetRepository<TotalEmployeeModel>())
                           .Returns(_mockRepo.Object);
            //_totalEmployeeService = new TotalEmployeeService(
            //    _mockUnitOfWork.Object
            //);

            //_controller = new AreaActivityApiController(_mockUnitOfWork.Object);

            //typeof(CommonControllerBase)
            //    .GetProperty("ServiceProxy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            //    ?.SetValue(_controller, _mockServiceProxy.Object);
        }

        #region GetFromTotalEmployee Tests

        [Fact]
        public void GetFromTotalEmployee_ReturnResults()
        {
            // Arrange
            var model = new TotalEmployeeModel
            {
                Id = Guid.NewGuid(),
                TotalEmployee = 100,
                TotalEmployeeOutsourcing = 50,
                DivisionCode = "DIV001",
                DivisionName = "Division 1",
                AreaId = Guid.NewGuid(),
                AreaName = "Area 1",
                TotalWorkDay = 20,
                TotalOvertime = 10,
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            var service = new TotalEmployeeService(_mockUnitOfWork.Object);

            // Act
            service.Upsert(model);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        #endregion

        #region CreateTotalEmployee Tests

        [Fact]
        public void CreateTotalEmployee_CreatedResult()
        {
            // Arrange
            var model = new TotalEmployeeModel
            {
                Id = Guid.NewGuid(),
                TotalEmployee = 100,
                TotalEmployeeOutsourcing = 50,
                DivisionCode = "DIV001",
                DivisionName = "Division 1",
                AreaId = Guid.NewGuid(),
                AreaName = "Area 1",
                TotalWorkDay = 20,
                TotalOvertime = 10,
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            var service = new TotalEmployeeService(_mockUnitOfWork.Object);

            // Act
            service.Upsert(model);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        #endregion

        #region UpdateTotalEmployee Tests

        [Fact]
        public void UpdateTotalEmployee_Result()
        {
            // Arrange
            var existingId = Guid.Parse("E0393AC6-B6F0-4BA8-8014-FC19646BD1E2");
            var areaId = Guid.NewGuid();

            var model = new TotalEmployeeModel
            {
                Id = existingId,
                TotalEmployee = 150,
                TotalEmployeeOutsourcing = 75,
                DivisionCode = "DIV001",
                DivisionName = "Division 1",
                AreaId = areaId,
                AreaName = "Area 1",
                TotalWorkDay = 25,
                TotalOvertime = 15,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true
            };

            var mockRepo = _mockRepo;

            var service = new TotalEmployeeService(_mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.Is<TotalEmployeeModel>(m =>
            m.Id == existingId &&
            m.TotalEmployee == 150 &&
            m.TotalEmployeeOutsourcing == 75 &&
            m.DivisionCode == "DIV001"
        ), It.Is<string[]>(fields =>
            fields.Contains("TotalEmployee") &&
            fields.Contains("TotalEmployeeOutsourcing") &&
            fields.Contains("TotalWorkDay") &&
            fields.Contains("TotalOvertime") &&
            fields.Contains("DivisionCode") &&
            fields.Contains("DivisionName") &&
            fields.Contains("AreaId") &&
            fields.Contains("AreaName") &&
            fields.Contains("RowStatus") &&
            fields.Contains("DeletedOn")
        )), Times.Once);



            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
        #endregion

        [Fact]
        public void GetSafetyIncident_ReturnResult()
        {
            // Arrange
            var model = new SafetyIncidentModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<SafetyIncidentModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockRepo.Setup(r => r.Fetch()).Returns(new List<SafetyIncidentModel> { model }.AsQueryable());
            mockUnitOfWork.Setup(u => u.GetRepository<SafetyIncidentModel>()).Returns(mockRepo.Object);

            var service = new SafetyIncidentService(mockUnitOfWork.Object);

            // Act
            service.UpdateSafetyIncident(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<SafetyIncidentModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void InsertSafetyIncident_Should_Add_And_Save()
        {
            // Arrange
            var model = new SafetyIncidentModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<SafetyIncidentModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockRepo.Setup(r => r.Fetch()).Returns(new List<SafetyIncidentModel> { model }.AsQueryable());
            mockUnitOfWork.Setup(u => u.GetRepository<SafetyIncidentModel>()).Returns(mockRepo.Object);

            var service = new SafetyIncidentService(mockUnitOfWork.Object);

            // Act
            service.UpdateSafetyIncident(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<SafetyIncidentModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateSafetyIncident_Should_Upsert_And_Save()
        {
            // Arrange
            var model = new SafetyIncidentModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<SafetyIncidentModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockRepo.Setup(r => r.Fetch()).Returns(new List<SafetyIncidentModel> { model }.AsQueryable());
            mockUnitOfWork.Setup(u => u.GetRepository<SafetyIncidentModel>()).Returns(mockRepo.Object);

            var service = new SafetyIncidentService(mockUnitOfWork.Object);

            // Act
            service.UpdateSafetyIncident(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<SafetyIncidentModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetSafetyFacility_ReturnResult()
        {
            // Arrange
            var model = new SafetyFacilityModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<SafetyFacilityModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<SafetyFacilityModel>()).Returns(mockRepo.Object);

            var service = new SafetyFacilityService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<SafetyFacilityModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpsertSafetyFacility_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new SafetyFacilityModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<SafetyFacilityModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<SafetyFacilityModel>()).Returns(mockRepo.Object);

            var service = new SafetyFacilityService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<SafetyFacilityModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetFireProtection_ReturnResult()
        {
            // Arrange
            var model = new FireProtectionModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<FireProtectionModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<FireProtectionModel>()).Returns(mockRepo.Object);

            var service = new FireProtectionService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<FireProtectionModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }


        [Fact]
        public void UpsertFireProtection_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new FireProtectionModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<FireProtectionModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<FireProtectionModel>()).Returns(mockRepo.Object);

            var service = new FireProtectionService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<FireProtectionModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetAPARRefill_ReturnResult()
        {
            // Arrange
            var model = new APARRefillModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<APARRefillModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<APARRefillModel>()).Returns(mockRepo.Object);

            var service = new APARRefillService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<APARRefillModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpsertAPARRefill_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new APARRefillModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<APARRefillModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<APARRefillModel>()).Returns(mockRepo.Object);

            var service = new APARRefillService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<APARRefillModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetTrainingRecord_ReturnResult()
        {
            // Arrange
            var model = new TrainingRecordModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<TrainingRecordModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<TrainingRecordModel>()).Returns(mockRepo.Object);

            var service = new TrainingRecordService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<TrainingRecordModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpsertTrainingRecord_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new TrainingRecordModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<TrainingRecordModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<TrainingRecordModel>()).Returns(mockRepo.Object);

            var service = new TrainingRecordService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<TrainingRecordModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetProjectActivity_ReturnResult()
        {
            // Arrange
            var model = new ProjectActivityModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<ProjectActivityModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<ProjectActivityModel>()).Returns(mockRepo.Object);

            var service = new ProjectActivityService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<ProjectActivityModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }


        [Fact]
        public void UpsertProjectActivity_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new ProjectActivityModel { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<ProjectActivityModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<ProjectActivityModel>()).Returns(mockRepo.Object);

            var service = new ProjectActivityService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<ProjectActivityModel>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

    }

    internal interface IReadOnlyRepository<T>
    {
    }

    public class UserClaim
    {
        public string NoReg { get; set; }
    }

    

    public interface IServiceProxy
    {
        T GetService<T>() where T : class;
        object GetService(Type type);
        UserClaim UserClaim { get; }
    }

    public interface ITotalEmployeeService
    {
        TotalEmployeeViewModel GetById(Guid id);
        void Upsert(TotalEmployeeModel model);
        void Delete(Guid id);
        IEnumerable<TotalEmployeeViewModel> Gets(string division, string area, string periode, string noreg);
    }

}