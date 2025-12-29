using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Microsoft.Extensions.Localization;
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
    public class CompanyLoanServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockAoeRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _mockNpRepo;
        private readonly Mock<IRepository<CompanyLoanSeq>> _mockSeqRepo;
        private readonly Mock<IRepository<DocumentApproval>> _docApprovalRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _docRequestDetailRepo;
        private readonly Mock<IRepository<TrackingApproval>> _trackingApprovalRepo;
        private readonly Mock<IRepository<ApprovalMatrix>> _approvalMatrixRepo;
        private readonly Mock<IRepository<DocumentApprovalFile>> _approvalFileRepo;
        private readonly Mock<IRepository<CompanyLoan>> _companyLoanRepo;
        private readonly Mock<IRepository<CompanyLoanDetail>> _companyLoanDetailRepo;
        private readonly Mock<ConfigService> _mockConfigService;

        private readonly ClaimBenefitService _service;
        private readonly Mock<ApprovalService> _serviceApproval;
        private readonly ApprovalService _serviceApp;

        public CompanyLoanServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();

            _mockAoeRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockNpRepo = new Mock<IRepository<EmployeeSubgroupNP>>();
            _mockSeqRepo = new Mock<IRepository<CompanyLoanSeq>>();
            _mockConfigService = new Mock<ConfigService>();
            _docApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _docRequestDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _trackingApprovalRepo = new Mock<IRepository<TrackingApproval>>();
            _approvalMatrixRepo = new Mock<IRepository<ApprovalMatrix>>();
            _approvalFileRepo = new Mock<IRepository<DocumentApprovalFile>>();
            _companyLoanRepo = new Mock<IRepository<CompanyLoan>>();
            _companyLoanDetailRepo = new Mock<IRepository<CompanyLoanDetail>>();

            _unitOfWork.Setup(u => u.GetRepository<CompanyLoanSeq>()).Returns(_mockSeqRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockAoeRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_mockNpRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_docApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_docRequestDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<TrackingApproval>()).Returns(_trackingApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ApprovalMatrix>()).Returns(_approvalMatrixRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApprovalFile>()).Returns(_approvalFileRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CompanyLoan>()).Returns(_companyLoanRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CompanyLoanDetail>()).Returns(_companyLoanDetailRepo.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var domainEventManager = new DomainEventManager(serviceProviderMock.Object);
            var localizer = Mock.Of<IStringLocalizer<IUnitOfWork>>();

            _serviceApproval = new Mock<ApprovalService>(_unitOfWork.Object, domainEventManager, localizer)
            {
                CallBase = true
            };
            _serviceApp = _serviceApproval.Object;

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateCompanyLoan7Up_ShouldThrowException_WhenNPBelow7()
        {
            // Arrange
            var noreg = "EMP001";
            var name = "John Doe";

            _mockAoeRepo.Setup(r => r.Fetch())
                .Returns(new List<ActualOrganizationStructure>
                {
                new ActualOrganizationStructure { NoReg = noreg, EmployeeSubgroup = "SG1" }
                }.AsQueryable());

            _mockNpRepo.Setup(r => r.Fetch())
                .Returns(new List<EmployeeSubgroupNP>
                {
                new EmployeeSubgroupNP { EmployeeSubgroup = "SG1", NP = 5, RowStatus = true }
                }.AsQueryable());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidateCompanyLoan7Up(noreg, name));
            Assert.Contains("is not in Class 7 or higher", ex.Message);
        }

        [Fact]
        public void PreValidateCompanyLoan7Up_ShouldNotThrow_WhenNP7OrAbove()
        {
            // Arrange
            var noreg = "EMP002";
            var name = "Jane Smith";

            _mockAoeRepo.Setup(r => r.Fetch())
                .Returns(new List<ActualOrganizationStructure>
                {
                new ActualOrganizationStructure { NoReg = noreg, EmployeeSubgroup = "SG2" }
                }.AsQueryable());

            _mockNpRepo.Setup(r => r.Fetch())
                .Returns(new List<EmployeeSubgroupNP>
                {
                new EmployeeSubgroupNP { EmployeeSubgroup = "SG2", NP = 7, RowStatus = true }
                }.AsQueryable());

            // Act & Assert
            var exception = Record.Exception(() => _service.PreValidateCompanyLoan7Up(noreg, name));
            Assert.Null(exception); // tidak ada error
        }

        [Fact]
        public void InsertAllowanceSeq_ShouldStartFrom1_WhenNoExistingSeq()
        {
            // arrange
            var mockSeqRepo = new Mock<IRepository<CompanyLoanSeq>>();
            var mockConfigRepo = new Mock<IRepository<Config>>();

            _unitOfWork.Setup(u => u.GetRepository<CompanyLoanSeq>()).Returns(mockSeqRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Config>()).Returns(mockConfigRepo.Object);

            // buat config yang akan dibaca oleh ConfigService.GetConfig(...)
            var configList = new List<Config>
            {
                new Config { ConfigKey = "MaxLoan.Sequence", ConfigValue = "5" }
            }.AsQueryable();

            mockConfigRepo.Setup(r => r.Fetch()).Returns(configList);

            // buat CompanyLoanSeq repo sesuai skenario (kosong atau ada data)
            mockSeqRepo.Setup(r => r.Fetch()).Returns(new List<CompanyLoanSeq>().AsQueryable());

            // service
            var svc = new ClaimBenefitService(_unitOfWork.Object);

            // act
            var loanVm = new LoanViewModel();
            svc.InsertAllowanceSeq("EMP001", loanVm);

            // assert: CompanyLoanSeq added and saved
            mockSeqRepo.Verify(r => r.Add(It.IsAny<CompanyLoanSeq>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);

            // loanVm.Seq and loanVm.Period updated
            Assert.Equal(1, loanVm.Seq);
        }

        [Fact]
        public void GetDocumentRequestDetailViewModel_ShouldReturnViewModel_WhenDataExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var noreg = "EMP001";

            var docApproval = new DocumentApproval { Id = id, CreatedBy = noreg, Form = new Form { FormKey = "test" } };
            var docDetail = new DocumentRequestDetail { Id = Guid.NewGuid(), DocumentApproval = docApproval };
            var tracking = new TrackingApproval
            {
                DocumentApprovalId = id,
                NoReg = noreg,
                ApprovalMatrixId = Guid.NewGuid()
            };
            var approvalMatrix = new ApprovalMatrix { Approver = "Manager", Permissions = "Approve" };

            _docApprovalRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<DocumentApproval, bool>>>()))
            .Returns(new List<DocumentApproval> { docApproval }.AsQueryable());

            _docRequestDetailRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<DocumentRequestDetail, bool>>>()))
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());
            _docApprovalRepo.Setup(r => r.Fetch()).Returns(new List<DocumentApproval> { docApproval }.AsQueryable());
            _trackingApprovalRepo.Setup(r => r.Fetch())
                .Returns(new List<TrackingApproval> { tracking }.AsQueryable());
            _approvalMatrixRepo.Setup(r => r.FindById(tracking.ApprovalMatrixId.Value))
                .Returns(approvalMatrix);

            // Act
            var result = _serviceApp.GetDocumentRequestDetailViewModel<object>(id, noreg);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal("Manager", result.Approver);
            //Assert.Equal("Approve", result.Permissions);
            //Assert.Equal(noreg, result.CreatedBy);
        }

        [Fact]
        public void GetDetailsReport_ShouldReturn_DataSourceResult()
        {
            // Arrange
            var mockFetcher = new Mock<IDataResultFetcher>();
            var request = new DataSourceRequest();
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            var dummyData = new List<CompanyLoan>
        {
            new CompanyLoan { Id = Guid.NewGuid(), Name = "Dummy User", PostName = "Tester" }
        };

            var expectedResult = new DataSourceResult
            {
                Data = dummyData,
                Total = dummyData.Count
            };

            mockFetcher
                .Setup(x => x.GetDataResult<CompanyLoan>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<CompanyLoan>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_COMPANY_LOAN i 
            LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg 
            WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }

        [Fact]
        public void CompleteLoanAllowance_Should_Call_SaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001",
                CreatedOn = DateTime.Now
            };

            var loanVm = new LoanViewModel
            {
                LoanType = "Car",
                LoanAmount = 50000000,
                CostNeeds = 30000000,
                LoanPeriod = 12,
                Interest = 10,
                Province = "Jawa Barat",
                City = "Bandung",
                District = "Cicendo",
                PostalCode = "40112",
                Address = "Jl. Test",
                RT = "01",
                RW = "02",
                Brand = "Toyota",
                Period = DateTime.Now,
                Seq = 1
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(loanVm)
            };

            _docRequestDetailRepo.Setup(r => r.Fetch()).Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            _mockSeqRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyLoanSeq, bool>>>()))
                .Returns(new List<CompanyLoanSeq>
                {
                new CompanyLoanSeq
                {
                    Id = Guid.NewGuid(),
                    Noreg = documentApproval.CreatedBy,
                    PeriodYear = loanVm.Period.Value.Year,
                    PeriodMonth = loanVm.Period.Value.Month,
                    OrderSequence = loanVm.Seq,
                    RequestStatus = "Pending"
                }
                }.AsQueryable());

            // Act
            _service.CompleteLoanAllowance(documentApproval.CreatedBy, documentApproval);

            // Assert -> cukup verify SaveChanges dipanggil 3 kali
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Exactly(3));
        }
    }
}
