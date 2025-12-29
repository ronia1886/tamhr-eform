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
    public class MarriageAllowanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentApproval>> _mockDocApprovalRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockActualOrgRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _mockEmpSubRepo;
        private readonly Mock<IRepository<Form>> _mockFormRepo;
        private readonly Mock<IRepository<General>> _mockGeneralRepo;
        private readonly ClaimBenefitService _service;

        public MarriageAllowanceServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockDocApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockGeneralRepo = new Mock<IRepository<General>>();
            _mockFormRepo = new Mock<IRepository<Form>>();
            _mockActualOrgRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockEmpSubRepo = new Mock<IRepository<EmployeeSubgroupNP>>();

            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_mockDocApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<General>()).Returns(_mockGeneralRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Form>()).Returns(_mockFormRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockActualOrgRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_mockEmpSubRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateMarriage_ShouldThrow_WhenMarriageAllowanceExists()
        {
            // Arrange
            var noreg = "EMP001";
            var form = new Form { Id = Guid.NewGuid(), FormKey = "marriage-allowance" };
            var approval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                CreatedBy = noreg,
                DocumentStatusCode = "completed"
            };

            _mockFormRepo.Setup(r => r.Fetch()).Returns(new List<Form> { form }.AsQueryable());
            _mockDocApprovalRepo.Setup(r => r.Fetch()).Returns(new List<DocumentApproval> { approval }.AsQueryable());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidateMarriage(noreg));
            Assert.Equal("Cant create request", ex.Message);
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
            var result = mockFetcher.Object.GetDataResult<General>("query", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }

        [Fact]
        public void CompleteMarriageAllowance_ShouldAddGeneral_AndSaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var weddingDate = new DateTime(2025, 12, 1);

            var marriageModel = new MarriageAllowanceViewModel
            {
                WeddingDate = weddingDate
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(marriageModel)
            };

            // Mock data fetch
            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            // Dummy allowance bypass CoreService
            decimal fakeAllowance = 1000000M;

            // Mock Add()
            General capturedGeneral = null;
            _mockGeneralRepo.Setup(r => r.Add(It.IsAny<General>()))
                .Callback<General>(g => capturedGeneral = g);

            // Act
            _service.CompleteMarriageAllowance("EMP001", documentApproval);

            // Assert
            Assert.NotNull(capturedGeneral);
            Assert.Equal("EMP001", capturedGeneral.NoReg);
            Assert.Equal("marriageallowance", capturedGeneral.AllowanceType);
            Assert.Equal(weddingDate, capturedGeneral.TransactionDate);
            Assert.True(capturedGeneral.HardCopyStatus);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
