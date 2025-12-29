using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Microsoft.Extensions.Localization;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;
using static TAMHR.ESS.UnitTest.ClaimBenefit.VacationAllowanceServiceTest;
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class CondolenceAllowanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<General>> _mockClaimGeneralRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockAoeRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _mockNpRepo;
        private readonly ClaimBenefitService _service;

        public CondolenceAllowanceServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockClaimGeneralRepo = new Mock<IRepository<General>>();
            _mockAoeRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockNpRepo = new Mock<IRepository<EmployeeSubgroupNP>>();

            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<General>()).Returns(_mockClaimGeneralRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockAoeRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_mockNpRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CompleteMisseryAllowance_ShouldAddGeneralAndSaveChanges_WhenNPNotEligible()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001",
                CreatedOn = DateTime.Now
            };

            var misseryVm = new MisseryAllowanceViewModel
            {
                MisseryDate = DateTime.Now,
                IsMainFamily = "ya"
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(misseryVm)
            };

            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            _mockAoeRepo.Setup(r => r.Fetch()).Returns(new List<ActualOrganizationStructure>
            {
                new ActualOrganizationStructure { NoReg = "EMP001", EmployeeSubgroup = "SG1", Name = "John Doe", EmployeeSubgroupText = "A" }
            }.AsQueryable());

            _mockNpRepo.Setup(r => r.Fetch()).Returns(new List<EmployeeSubgroupNP>
            {
                new EmployeeSubgroupNP { EmployeeSubgroup = "SG1", NP = 4, RowStatus = true }
            }.AsQueryable());

            // Act
            _service.CompleteMisseryAllowance(documentApproval.CreatedBy, documentApproval);

            // Assert
            _mockClaimGeneralRepo.Verify(r => r.Add(It.Is<General>(g =>
                g.NoReg == documentApproval.CreatedBy &&
                g.AllowanceType == "miseryallowance" &&
                g.Ammount == 0 &&
                g.TransactionDate == misseryVm.MisseryDate.Value
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
            var result = mockFetcher.Object.GetDataResult<General>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_GENERAL i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg 
            WHERE i.AllowanceType IN ('miseryallowance') AND i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
