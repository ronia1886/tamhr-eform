using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;
using static TAMHR.ESS.UnitTest.ClaimBenefit.VacationAllowanceServiceTest;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class CopFuelServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<CarPurchase>> _mockCarPurchaseRepo;
        private readonly Mock<IRepository<General>> _mockClaimGeneralRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<CopFuel>> _mockCopFuelRepo;
        private readonly ClaimBenefitService _service;

        public CopFuelServiceTest() 
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockCarPurchaseRepo = new Mock<IRepository<CarPurchase>>();
            _mockClaimGeneralRepo = new Mock<IRepository<General>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockCopFuelRepo = new Mock<IRepository<CopFuel>>();

            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CopFuel>()).Returns(_mockCopFuelRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CarPurchase>()).Returns(_mockCarPurchaseRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<General>()).Returns(_mockClaimGeneralRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateCopFuelAllowance_ShouldThrowException_WhenNoCopHistory()
        {
            // Arrange
            var noreg = "1001";
            var name = "John Doe";

            // repositori kosong → simulate user belum pernah request
            _mockCarPurchaseRepo.Setup(r => r.Fetch()).Returns(new List<CarPurchase>().AsQueryable());
            _mockClaimGeneralRepo.Setup(r => r.Fetch()).Returns(new List<General>().AsQueryable());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidateCopFuelAllowance(noreg, name));
            Assert.Contains("Can not request Claim COP fuel", ex.Message);
            Assert.Contains(name, ex.Message);
        }

        [Fact]
        public void PreValidateCopFuelAllowance_ShouldNotThrow_WhenCopHistoryExists()
        {
            // Arrange
            var noreg = "1001";
            var name = "John Doe";

            // repositori ada data → simulate user pernah request COP
            _mockCarPurchaseRepo.Setup(r => r.Fetch()).Returns(new List<CarPurchase>
        {
            new CarPurchase { NoReg = noreg, CarPurchaseType = "COP" }
        }.AsQueryable());

            _mockClaimGeneralRepo.Setup(r => r.Fetch()).Returns(new List<General>
        {
            new General { NoReg = noreg, AllowanceType = "cop" }
        }.AsQueryable());

            // Act & Assert
            var ex = Record.Exception(() => _service.PreValidateCopFuelAllowance(noreg, name));
            Assert.Null(ex); // tidak ada exception
        }

        [Fact]
        public void GetDetailsReport_ShouldReturn_DataSourceResult()
        {
            // Arrange
            var mockFetcher = new Mock<IDataResultFetcher>();
            var request = new DataSourceRequest();
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            var dummyData = new List<General>
            {
                new General { Id = Guid.NewGuid(), Name = "Dummy User", PostName = "Tester" }
            };

            var expectedResult = new DataSourceResult
            {
                Data = dummyData,
                Total = dummyData.Count
            };

            mockFetcher
                .Setup(x => x.GetDataResult<CopFuel>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<CopFuel>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_CAR_PURCHASE i 
            LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }

        [Fact]
        public void CompleteCopFuelAllowance_ShouldAddCopFuel_AndSaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001",
                CreatedOn = DateTime.Now
            };

            var copFuelData = new CopFuelAllowanceViewModel
            {
                data = new List<DataCopFuelAllowanceViewModel>
            {
                new DataCopFuelAllowanceViewModel
                {
                    Date = DateTime.Now,
                    Necessity = "Meeting",
                    Destination = "Office A",
                    Start = 10,
                    Back = 50
                },
                new DataCopFuelAllowanceViewModel
                {
                    Date = DateTime.Now.AddDays(1),
                    Necessity = "Site Visit",
                    Destination = "Office B",
                    Start = 5,
                    Back = 30
                }
            },
                Remarks = "Test Remarks"
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(copFuelData)
            };

            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            // Act
            _service.CompleteCopFuelAllowance("EMP001", documentApproval);

            // Assert
            _mockCopFuelRepo.Verify(r => r.Add(It.Is<CopFuel>(c =>
                c.NoReg == "EMP001" &&
                (c.Purpose == "Meeting" || c.Purpose == "Site Visit")
            )), Times.Exactly(2));

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
