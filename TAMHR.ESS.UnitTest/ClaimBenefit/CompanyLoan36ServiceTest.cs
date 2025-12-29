using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
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

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class CompanyLoan36ServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _aoeRepo;
        private readonly Mock<IRepository<EmployeeSubgroupNP>> _npRepo;
        private readonly Mock<IRepository<CompanyLoanSeq>> _companyLoanSeqRepo;
        private readonly Mock<IRepository<Config>> _configRepo;
        private readonly Mock<IRepository<DocumentApproval>> _docApprovalRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _docRequestDetailRepo;
        private readonly Mock<IRepository<TrackingApproval>> _trackingApprovalRepo;
        private readonly Mock<IRepository<ApprovalMatrix>> _approvalMatrixRepo;
        private readonly Mock<IRepository<CompanyLoan>> _companyLoanRepo;
        private readonly Mock<IRepository<CompanyLoanDetail>> _companyLoanDetailRepo;
        private readonly Mock<IRepository<DocumentApprovalFile>> _approvalFileRepo;

        ClaimBenefitService _service;
        private readonly Mock<ApprovalService> _serviceApproval;
        private readonly ApprovalService _serviceApp;

        public CompanyLoan36ServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _aoeRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _npRepo = new Mock<IRepository<EmployeeSubgroupNP>>();
            _companyLoanSeqRepo = new Mock<IRepository<CompanyLoanSeq>>();
            _configRepo = new Mock<IRepository<Config>>();
            _docApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _docRequestDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _trackingApprovalRepo = new Mock<IRepository<TrackingApproval>>();
            _approvalMatrixRepo = new Mock<IRepository<ApprovalMatrix>>();
            _companyLoanRepo = new Mock<IRepository<CompanyLoan>>();
            _companyLoanDetailRepo = new Mock<IRepository<CompanyLoanDetail>>();
            _approvalFileRepo = new Mock<IRepository<DocumentApprovalFile>>();

            _unitOfWork.Setup(u => u.GetRepository<DocumentApprovalFile>()).Returns(_approvalFileRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CompanyLoan>()).Returns(_companyLoanRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CompanyLoanDetail>()).Returns(_companyLoanDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ApprovalMatrix>()).Returns(_approvalMatrixRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_aoeRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<EmployeeSubgroupNP>()).Returns(_npRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<CompanyLoanSeq>()).Returns(_companyLoanSeqRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Config>()).Returns(_configRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_docApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_docRequestDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<TrackingApproval>()).Returns(_trackingApprovalRepo.Object);

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

        [Theory]
        [InlineData(2, true)]  // NP < 3 → throw
        [InlineData(3, false)] // NP = 3 → valid
        [InlineData(5, false)] // NP = 5 → valid
        [InlineData(6, false)] // NP = 6 → valid
        [InlineData(7, true)]  // NP ≥ 7 → throw
        public void PreValidateCompanyLoan36_Should_Validate_NP(int npValue, bool shouldThrow)
        {
            // Arrange
            var noreg = "EMP001";
            var name = "John Doe";

            _aoeRepo.Setup(r => r.Fetch()).Returns(new List<ActualOrganizationStructure>
        {
            new ActualOrganizationStructure { NoReg = noreg, EmployeeSubgroup = "SG1" }
        }.AsQueryable());

            _npRepo.Setup(r => r.Fetch()).Returns(new List<EmployeeSubgroupNP>
        {
            new EmployeeSubgroupNP { EmployeeSubgroup = "SG1", NP = npValue, RowStatus = true }
        }.AsQueryable());

            // Act & Assert
            if (shouldThrow)
            {
                var ex = Assert.Throws<Exception>(() => _service.PreValidateCompanyLoan36(noreg, name));
                Assert.Contains("is not in Between Class 3 or 6", ex.Message);
            }
            else
            {
                var exception = Record.Exception(() => _service.PreValidateCompanyLoan36(noreg, name));
                Assert.Null(exception); // tidak boleh throw
            }
        }
        [Fact]
        public void InsertAllowanceSeq36_Should_Add_New_Sequence_And_Set_ViewModel()
        {
            // Arrange
            var noreg = "EMP001";
            var vm = new Loan36ViewModel();

            var now = DateTime.Now;

            // Mock CompanyLoanSeqRepository (kosong -> sequence mulai dari 1)
            _companyLoanSeqRepo.Setup(r => r.Fetch())
                .Returns(new List<CompanyLoanSeq>().AsQueryable());

            // Mock ConfigRepository untuk "MaxLoan.Sequence"
            _configRepo.Setup(r => r.Fetch())
                .Returns(new List<Config>
                {
            new Config { ConfigKey = "MaxLoan.Sequence", ConfigValue = "5" }
                }.AsQueryable());

            // Act
            _service.InsertAllowanceSeq36(noreg, vm);

            // Assert
            _companyLoanSeqRepo.Verify(r => r.Add(It.Is<CompanyLoanSeq>(
                c => c.Noreg == noreg
                     && c.PeriodYear == now.Year
                     && c.PeriodMonth == now.Month
                     && c.OrderSequence == 1
                     && c.RequestStatus == "Pending"
            )), Times.Once);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);

            Assert.Equal(1, vm.Seq);
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
        public void CompleteLoanAllowance36_Should_Add_Loan_And_Detail_And_Update_Seq()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var loanVm = new Loan36ViewModel
            {
                LoanType = "Housing",
                LoanAmount = 12000000,
                CostNeeds = 6000000,
                LoanPeriod = 12,
                Interest = 10,
                Province = "DKI",
                City = "Jakarta",
                District = "Cempaka Putih",
                PostalCode = "10510",
                Address = "Jl. Contoh",
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

            var seq = new CompanyLoanSeq
            {
                Noreg = documentApproval.CreatedBy,
                PeriodYear = loanVm.Period.Value.Year,
                PeriodMonth = loanVm.Period.Value.Month,
                OrderSequence = loanVm.Seq,
                RequestStatus = "Pending"
            };

            _companyLoanSeqRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyLoanSeq, bool>>>()))
                .Returns(new List<CompanyLoanSeq> { seq }.AsQueryable());

            // Act
            _service.CompleteLoanAllowance36(documentApproval.CreatedBy, documentApproval);

            // Assert
            _companyLoanRepo.Verify(r => r.Add(It.Is<CompanyLoan>(c =>
                c.NoReg == documentApproval.CreatedBy &&
                c.LoanType == loanVm.LoanType &&
                c.AmmountLoan == loanVm.CostNeeds.Value
            )), Times.Once);

            _companyLoanDetailRepo.Verify(r => r.Add(It.Is<CompanyLoanDetail>(d =>
                d.City == loanVm.City &&
                d.Brand == loanVm.Brand
            )), Times.Once);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Exactly(3));
        }

        [Fact]
        public void RejectLoanAllowance36_Should_Delete_Seq_And_Update_Detail_When_Period_NotNull()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var loanVm = new Loan36ViewModel
            {
                Period = new DateTime(2025, 9, 1),
                Seq = 5
            };

            var detail = new DocumentRequestDetail
            {
                Id = Guid.NewGuid(),
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(loanVm)
            };

            _docRequestDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { detail }.AsQueryable());

            _companyLoanSeqRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyLoanSeq, bool>>>())).Returns(new List<CompanyLoanSeq> 
            {
                new CompanyLoanSeq
                {
                    Id = Guid.NewGuid(),
                    Noreg = documentApproval.CreatedBy,
                    PeriodYear = loanVm.Period.Value.Year,
                    PeriodMonth = loanVm.Period.Value.Month,
                    OrderSequence = loanVm.Seq
                }
            }.AsQueryable());

            // Act
            _service.RejectLoanAllowance36("EMP001", documentApproval);

            // Assert
            _companyLoanSeqRepo.Verify(r => r.Delete(It.IsAny<CompanyLoanSeq>()), Times.Once);
            _docRequestDetailRepo.Verify(r => r.Attach(It.Is<DocumentRequestDetail>(
                d => JsonConvert.DeserializeObject<Loan36ViewModel>(d.ObjectValue).Period == null
                  && JsonConvert.DeserializeObject<Loan36ViewModel>(d.ObjectValue).Seq == 0
            )), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void RejectLoanAllowance36_Should_DoNothing_When_Period_IsNull()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var loanVm = new Loan36ViewModel
            {
                Period = null,
                Seq = 0
            };

            var detail = new DocumentRequestDetail
            {
                Id = Guid.NewGuid(),
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(loanVm)
            };

            _docRequestDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { detail }.AsQueryable());

            // Act
            _service.RejectLoanAllowance36("EMP001", documentApproval);

            // Assert
            _companyLoanSeqRepo.Verify(r => r.Delete(It.IsAny<CompanyLoanSeq>()), Times.Never);
            _docRequestDetailRepo.Verify(r => r.Attach(It.IsAny<DocumentRequestDetail>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Never);
        }
    }
}
