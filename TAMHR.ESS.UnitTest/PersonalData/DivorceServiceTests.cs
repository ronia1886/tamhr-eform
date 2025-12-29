using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
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
using PersonalDataEntity = TAMHR.ESS.Domain.PersonalData;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class DivorceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<PersonalDataEntity>> _mockPersonalDataRepo;
        private readonly Mock<IRepository<PersonalDataFamilyMember>> _mockFamilyRepo;
        private readonly Mock<IRepository<PersonalDataCommonAttribute>> _mockCommonAttrRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockOrgRepo;
        private readonly Mock<IRepository<Notification>> _mockNotifRepo;
        private readonly Mock<IRepository<User>> _mockUserRepo;
        private readonly Mock<IRepository<UserRole>> _mockUserRoleRepo;
        private readonly Mock<IRepository<GeneralCategory>> _mockGeneralCategoryRepo;
        private readonly Mock<IRepository<PersonalDataBpjs>> _mockPersonalDataBpjsRepo;
        private readonly Mock<IRepository<PersonalDataInsurance>> _mockPersonalDataInsurance;
        private readonly Mock<IRepository<PersonalDataEvent>> _mockPersonalDataEvent;
        private readonly Mock<IRepository<DocumentApprovalFile>> _mockDocumentApprovalFile;
        private readonly Mock<IRepository<PersonalDataTaxStatus>> _mockPersonalDataTaxStatus;

        private readonly PersonalDataService _service;

        public DivorceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockPersonalDataRepo = new Mock<IRepository<PersonalDataEntity>>();
            _mockFamilyRepo = new Mock<IRepository<PersonalDataFamilyMember>>();
            _mockCommonAttrRepo = new Mock<IRepository<PersonalDataCommonAttribute>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockOrgRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockGeneralCategoryRepo = new Mock<IRepository<GeneralCategory>>();
            _mockPersonalDataBpjsRepo = new Mock<IRepository<PersonalDataBpjs>>();
            _mockPersonalDataInsurance = new Mock<IRepository<PersonalDataInsurance>>();
            _mockPersonalDataEvent = new Mock<IRepository<PersonalDataEvent>>();
            _mockDocumentApprovalFile = new Mock<IRepository<DocumentApprovalFile>>();
            _mockPersonalDataTaxStatus = new Mock<IRepository<PersonalDataTaxStatus>>();
            _mockNotifRepo = new Mock<IRepository<Notification>>();

            // Setup semua repository agar tidak null
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataEntity>()).Returns(_mockPersonalDataRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataFamilyMember>()).Returns(_mockFamilyRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataCommonAttribute>()).Returns(_mockCommonAttrRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockOrgRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<Notification>()).Returns(_mockNotifRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<GeneralCategory>()).Returns(_mockGeneralCategoryRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataBpjs>()).Returns(_mockPersonalDataBpjsRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataInsurance>()).Returns(_mockPersonalDataInsurance.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataEvent>()).Returns(_mockPersonalDataEvent.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<DocumentApprovalFile>()).Returns(_mockDocumentApprovalFile.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataTaxStatus>()).Returns(_mockPersonalDataTaxStatus.Object);

            _mockUserRepo = new Mock<IRepository<User>>(MockBehavior.Strict);
            _mockUserRoleRepo = new Mock<IRepository<UserRole>>(MockBehavior.Strict);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<User>())
                .Returns(_mockUserRepo.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<UserRole>())
                .Returns(_mockUserRoleRepo.Object);

            

            var user = new User { Id = Guid.NewGuid(), Username = "SHE Approver" };
            var role = new Role { RoleKey = "SHE" };
            var userRole = new UserRole { UserId = user.Id, User = user, Role = role };

            _mockUserRepo.Setup(r => r.Fetch()).Returns(new[] { user }.AsQueryable());
            _mockUserRoleRepo.Setup(r => r.Fetch()).Returns(new[] { userRole }.AsQueryable());

            // Default SaveChanges()
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            _service = new PersonalDataService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void CompleteDivorce_WhenFemaleEmployee_ShouldUpdateMaritalStatusToJanda()
        {
            // Arrange
            var noregApprover = "EMP002";

            var personalData = new PersonalDataEntity
            {
                Id = Guid.NewGuid(),
                NoReg = "EMP001",
                CommonAttributeId = Guid.NewGuid(),
                MaritalStatusCode = "menikah"
            };

            var commonAttr = new PersonalDataCommonAttribute
            {
                Id = personalData.CommonAttributeId,
                GenderCode = "perempuan"
            };

            var divorceVm = new DivorceViewModel
            {
                PartnerId = Guid.NewGuid().ToString(),
                DivorceDate = DateTime.Today
            };

            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var docRequestDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(divorceVm)
            };

            var partnerFamily = new PersonalDataFamilyMember
            {
                Id = Guid.Parse(divorceVm.PartnerId),
                RowStatus = true
            };

            // ✅ Mock semua fetch yang dipakai
            _mockPersonalDataRepo.Setup(r => r.Fetch()).Returns(new[] { personalData }.AsQueryable());
            _mockCommonAttrRepo.Setup(r => r.FindById(personalData.CommonAttributeId)).Returns(commonAttr);
            _mockDocDetailRepo.Setup(r => r.Fetch()).Returns(new[] { docRequestDetail }.AsQueryable());
            _mockFamilyRepo.Setup(r => r.Fetch()).Returns(new[] { partnerFamily }.AsQueryable());

            // Setup Upsert agar tidak null reference
            _mockPersonalDataRepo.Setup(r => r.Upsert<Guid>(It.IsAny<PersonalDataEntity>()));
            _mockFamilyRepo.Setup(r => r.Upsert<Guid>(It.IsAny<PersonalDataFamilyMember>()));

            var taxStatus = new PersonalDataTaxStatus
            {
                Id = Guid.NewGuid(),
                NoReg = documentApproval.CreatedBy, // "EMP001"
                Npwp = "123456789",
                TaxStatus = "K0",
                RowStatus = true,
                StartDate = DateTime.Now.AddYears(-1),
                EndDate = DateTime.Now.AddYears(1)
            };

            _mockPersonalDataTaxStatus
                .Setup(r => r.Fetch())
                .Returns(new[] { taxStatus }.AsQueryable());

            // Act
            _service.CompleteDivorce(noregApprover, documentApproval);

            // Assert
            _mockPersonalDataRepo.Verify(r => r.Upsert<Guid>(
                It.Is<PersonalDataEntity>(p => p.MaritalStatusCode == "janda")), Times.Once);

            _mockFamilyRepo.Verify(r => r.Upsert<Guid>(
                It.Is<PersonalDataFamilyMember>(f => f.EndDate == divorceVm.DivorceDate)), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.AtLeastOnce);
        }
    }
}
