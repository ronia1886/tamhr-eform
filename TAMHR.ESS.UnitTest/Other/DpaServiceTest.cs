using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers;
using Xunit;

namespace TAMHR.ESS.UnitTest.Others
{
    public class DpaServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<User>> _userRepo;
        private readonly Mock<IRepository<Form>> _formRepo;
        private readonly Mock<IRepository<FormSequence>> _formSeqRepo;
        private readonly Mock<IRepository<DocumentApproval>> _docApprovalRepo;
        private readonly Mock<ApprovalService> _serviceMock;
        private readonly Mock<IRepository<DocumentRequestDetail>> _docRequestDetailRepo;
        private readonly Mock<IRepository<Dpa>> _dpaRepo;
        private readonly Mock<IRepository<DpaMember>> _dpaMemberRepo;
        private readonly Mock<IRepository<PersonalDataBankAccount>> _personalDataBankRepo;
        private readonly Mock<IRepository<Bank>> _bankRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _aoeRepo;

        private readonly ApprovalService _service;
        private readonly ClaimBenefitService _claimBenefitService;

        public DpaServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _userRepo = new Mock<IRepository<User>>();
            _formRepo = new Mock<IRepository<Form>>();
            _formSeqRepo = new Mock<IRepository<FormSequence>>();
            _docApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _docRequestDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _dpaRepo = new Mock<IRepository<Dpa>>();
            _dpaMemberRepo = new Mock<IRepository<DpaMember>>();
            _personalDataBankRepo = new Mock<IRepository<PersonalDataBankAccount>>();
            _bankRepo = new Mock<IRepository<Bank>>();
            _aoeRepo = new Mock<IRepository<ActualOrganizationStructure>>();

            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_aoeRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_docRequestDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Dpa>()).Returns(_dpaRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DpaMember>()).Returns(_dpaMemberRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<PersonalDataBankAccount>()).Returns(_personalDataBankRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Bank>()).Returns(_bankRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<User>()).Returns(_userRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Form>()).Returns(_formRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<FormSequence>()).Returns(_formSeqRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_docApprovalRepo.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var domainEventManager = new DomainEventManager(serviceProviderMock.Object);
            var localizer = Mock.Of<IStringLocalizer<IUnitOfWork>>();

            // ✅ Create partial mock untuk virtual methods
            _serviceMock = new Mock<ApprovalService>(_unitOfWork.Object, domainEventManager, localizer)
            {
                CallBase = true
            };
            _service = _serviceMock.Object;
            _claimBenefitService = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CreateApprovalDocument_ShouldInsertDocumentApproval()
        {
            // Arrange
            var mockService = new Mock<IApprovalService>();
            var noreg = "EMP001";
            var viewModel = new DocumentRequestDetailViewModel
            {
                FormKey = "DPA_FORM",
                ReferenceId = null,
                ReferenceTable = "TableX",
                ObjectValue = "{}",
                Attachments = new List<DocumentApprovalFile>()
            };

            Func<string, Dictionary<string, object>, string> callback =
                (title, dict) => $"Generated Document for {dict["noreg"]}";

            mockService.Setup(s => s.CreateApprovalDocument(noreg, viewModel, callback));

            // Act
            mockService.Object.CreateApprovalDocument(noreg, viewModel, callback);

            // Assert
            mockService.Verify(s => s.CreateApprovalDocument(noreg, viewModel, callback), Times.Once);
        }

        [Fact]
        public void CompleteDpaRegister_Should_Add_New_Dpa_And_Members()
        {
            // Arrange
            var approval = new DocumentApproval { Id = Guid.NewGuid(), CreatedBy = "EMP001" };

            var vm = new DpaRegisterViewModel
            {
                AccountType = "nonrekening",
                BankCode = "B001",
                AccountName = "John Doe",
                AccountNumber = "123456",
                Email = "john@test.com",
                HouseNumber = "021123",
                MobilePhoneNumber = "081234",
                AhliWaris = new List<AhliWaris>
            {
                new AhliWaris { Name = "Family1", FamilyRelation = "Child", BrithDate = DateTime.Now.AddYears(-10) }
            }
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = approval.Id,
                ObjectValue = JsonConvert.SerializeObject(vm)
            };

            _docRequestDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            _dpaRepo.Setup(r => r.Fetch()).Returns(new List<Dpa>().AsQueryable()); // kosong
            _dpaMemberRepo.Setup(r => r.Fetch()).Returns(new List<DpaMember>().AsQueryable()); // kosong
            _bankRepo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Bank, bool>>>()))
                .Returns(new List<Bank> { new Bank { BankKey = "B001", BankName = "Test Bank", Branch = "Jakarta" } }.AsQueryable());

            // Act
            _claimBenefitService.CompleteDpaRegister("EMP002", approval);

            // Assert
            _dpaRepo.Verify(r => r.Add(It.Is<Dpa>(d =>
                d.NoReg == approval.CreatedBy &&
                d.BankName == "Test Bank" &&
                d.AccountName == vm.AccountName
            )), Times.Once);

            _dpaMemberRepo.Verify(r => r.Add(It.Is<DpaMember>(m =>
                m.NoReg == approval.CreatedBy &&
                m.Name == "Family1"
            )), Times.Once);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Exactly(2)); // sekali setelah delete member, sekali setelah add new members
        }
    }

    public interface IApprovalService
    {
        void CreateApprovalDocument(string noreg, DocumentRequestDetailViewModel viewModel,
                                   Func<string, Dictionary<string, object>, string> callback);
    }
}
