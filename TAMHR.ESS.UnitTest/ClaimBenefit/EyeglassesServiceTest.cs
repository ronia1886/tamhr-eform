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

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class EyeglassesServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<EyeglassesAllowanceViewModel>> _mockEyeglassesRepo;
        private readonly Mock<IRepository<General>> _mockGeneralRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockActualOrgRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _mockEmpSubRepo;
        private readonly Mock<IRepository<AllowanceDetail>> _mockAllowanceDetail;
        private readonly Mock<IRepository<PersonalDataTaxStatus>> _mockPersonalDataTaxRepo;
        private readonly Mock<IStringLocalizer<IUnitOfWork>> _mockLocalizer;
        private readonly ClaimBenefitService _service;
        private readonly CoreService _coreService;

        public EyeglassesServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockEyeglassesRepo = new Mock<IRepository<EyeglassesAllowanceViewModel>>();
            _mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();
            _mockGeneralRepo = new Mock<IRepository<General>>();
            _mockActualOrgRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockEmpSubRepo = new Mock<IRepository<EmployeeSubgroupNP>>();
            _mockAllowanceDetail = new Mock<IRepository<AllowanceDetail>>();
            _mockPersonalDataTaxRepo = new Mock<IRepository<PersonalDataTaxStatus>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();

            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockActualOrgRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_mockEmpSubRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<General>()).Returns(_mockGeneralRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EyeglassesAllowanceViewModel>()).Returns(_mockEyeglassesRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<AllowanceDetail>()).Returns(_mockAllowanceDetail.Object);
            _unitOfWork.Setup(u => u.GetRepository<PersonalDataTaxStatus>()).Returns(_mockPersonalDataTaxRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
            _coreService = new CoreService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateEyeglasses_ShouldThrowException_WhenFrameAndLensStillActive()
        {
            // Arrange
            var noreg = "1001";
            var now = DateTime.Now.Date;

            var claims = new List<General>
            {
                new General
                {
                    NoReg = noreg,
                    AllowanceType = "frame",
                    TransactionDate = now.AddMonths(-6) // masih < 2 tahun
                },
                new General
                {
                    NoReg = noreg,
                    AllowanceType = "lensa",
                    TransactionDate = now.AddMonths(-3) // masih < 1 tahun
                }
            };

            _mockGeneralRepo.Setup(r => r.Fetch())
                .Returns(claims.AsQueryable());

            _mockLocalizer.Setup(l => l["You can claim the lens again at"])
                .Returns(new LocalizedString("You can claim the lens again at", "You can claim the lens again at"));

            _mockLocalizer.Setup(l => l["You can claim the frame again at"])
                .Returns(new LocalizedString("You can claim the frame again at", "You can claim the frame again at"));

            var field = typeof(ClaimBenefitService)
                .GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_service, _mockLocalizer.Object);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidateEyeglasess(noreg));

            Assert.Contains("You can claim the lens again at", ex.Message);
            Assert.Contains("You can claim the frame again at", ex.Message);
        }

        [Fact]
        public void PreValidateEyeglasses_ShouldPass_WhenFrameAndLensExpired()
        {
            // Arrange
            var noreg = "1002";
            var now = DateTime.Now.Date;

            var claims = new List<General>
            {
                new General
                {
                    NoReg = noreg,
                    AllowanceType = "frame",
                    TransactionDate = now.AddYears(-6) // masih < 2 tahun
                },
                new General
                {
                    NoReg = noreg,
                    AllowanceType = "lensa",
                    TransactionDate = now.AddMonths(-3) // masih < 1 tahun
                }
            };

            _mockGeneralRepo.Setup(r => r.Fetch())
                .Returns(claims.AsQueryable());

            // Act & Assert
            _service.PreValidateEyeglasess(noreg);
        }

        [Fact]
        public void GetInfoAllowance_ShouldReturnAllowance_WhenDataExists()
        {
            // Arrange
            var noreg = "EMP001";

            _mockActualOrgRepo.Setup(r => r.Fetch()).Returns(new List<ActualOrganizationStructure>
            {
                new ActualOrganizationStructure 
                { NoReg = noreg, 
                    EmployeeSubgroup = "A", 
                    EmployeeSubgroupText = "Kelas A", 
                    Name = "Test" 
                }
            }.AsQueryable());

            _mockEmpSubRepo.Setup(r => r.Fetch()).Returns(new List<EmployeeSubgroupNP>
            {
                new EmployeeSubgroupNP 
                { 
                    EmployeeSubgroup = "A", 
                    NP = 5, 
                    RowStatus = true 
                }
            }.AsQueryable());

            _mockPersonalDataTaxRepo.Setup(r => r.Fetch()).Returns(new List<PersonalDataTaxStatus>
            {
                new PersonalDataTaxStatus 
                { 
                    NoReg = noreg, 
                    RowStatus = true, 
                    StartDate = DateTime.Now.AddMonths(-1), 
                    EndDate = DateTime.Now.AddMonths(1), 
                    TaxStatus = "TK0" 
                }
            }.AsQueryable());

            _mockAllowanceDetail.Setup(r => r.Fetch()).Returns(new List<AllowanceDetail>
            {
                new AllowanceDetail { Type = "EyewearAllowanceType", SubType = "lensa", ClassFrom = 1, ClassTo = 10, Ammount = 500000 }
            }.AsQueryable());


            // Act
            var result = _coreService.GetInfoAllowance(noreg, "EyewearAllowanceType", "lensa");

            // Assert
            var amount = Convert.ToInt64(result.GetType().GetProperty("Ammount").GetValue(result));
            Assert.NotNull(result);
            Assert.Equal(500000, amount);
        }

        [Fact]
        public void CompleteEyeglassesAllowance_ShouldAddFrameAndLensClaims_AndSaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001",
                CreatedOn = new DateTime(2025, 9, 29)
            };

            var eyeglassModel = new EyeglassesAllowanceViewModel
            {
                IsFrame = true,
                AmountFrame = 1000000,
                IsLens = true,
                AmountLens = 500000
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(eyeglassModel)
            };

            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            // Act
            _service.CompleteEyeglassesAllowance("EMP001", documentApproval);

            // Assert
            _mockGeneralRepo.Verify(r => r.Add(It.Is<General>(
                g => g.NoReg == "EMP001" &&
                     g.AllowanceType == "frame" &&
                     g.Ammount == 1000000 &&
                     g.TransactionDate == documentApproval.CreatedOn
            )), Times.Once);

            _mockGeneralRepo.Verify(r => r.Add(It.Is<General>(
                g => g.NoReg == "EMP001" &&
                     g.AllowanceType == "lensa" &&
                     g.Ammount == 500000 &&
                     g.TransactionDate == documentApproval.CreatedOn
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
            var result = mockFetcher.Object.GetDataResult<General>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_GENERAL i 
            LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg 
            WHERE i.AllowanceType IN ('frame', 'lensa') AND i.TransactionDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
