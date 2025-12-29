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
    public class DistressedAllowanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentRequestDetail>> _docRequestDetailRepo;
        private readonly Mock<IRepository<Distressed>> _distressedRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockAoeRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _mockNpRepo;

        private readonly ClaimBenefitService _service;
        public DistressedAllowanceServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();

            _docRequestDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _distressedRepo = new Mock<IRepository<Distressed>>();
            _mockAoeRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockNpRepo = new Mock<IRepository<EmployeeSubgroupNP>>();

            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockAoeRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_mockNpRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_docRequestDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Distressed>()).Returns(_distressedRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CompleteDistressedAllowance_Should_Add_Distressed_And_SaveChanges()
        {
            // Arrange
            var approval = new DocumentApproval { Id = Guid.NewGuid(), CreatedBy = "EMP001" };
            var vm = new DistressedAllowanceViewModel
            {
                Description = "Test Case",
                DateDistressed = DateTime.Now
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = approval.Id,
                ObjectValue = JsonConvert.SerializeObject(vm)
            };

            _docRequestDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            // Act
            _service.CompleteDistressedAllowance("EMP002", approval);

            // Assert
            _distressedRepo.Verify(r => r.Add(It.Is<Distressed>(d =>
                d.NoReg == approval.CreatedBy &&
                d.Description == vm.Description
            )), Times.Once);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
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
                .Setup(x => x.GetDataResult<General>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<General>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_DISTRESSED i 
            LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
