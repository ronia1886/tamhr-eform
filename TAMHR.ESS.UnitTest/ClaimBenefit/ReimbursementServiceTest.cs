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
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class ReimbursementServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentApproval>> _mockDocApprovalRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<ClaimHospital>> _mockClaimHospitalRepo;
        private readonly Mock<IRepository<PersonalDataBankAccount>> _mockPersonalDataBankAccountRepo;
        private readonly Mock<IRepository<Bank>> _mockBankRepo;
        private readonly Mock<IRepository<User>> _mockUserRepo;
        private readonly ClaimBenefitService _service;

        public ReimbursementServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockDocApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockClaimHospitalRepo = new Mock<IRepository<ClaimHospital>>();
            _mockPersonalDataBankAccountRepo = new Mock<IRepository<PersonalDataBankAccount>>();
            _mockBankRepo = new Mock<IRepository<Bank>>();
            _mockUserRepo = new Mock<IRepository<User>>();

            // Setup GetRepository untuk UnitOfWork
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_mockDocApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ClaimHospital>()).Returns(_mockClaimHospitalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<PersonalDataBankAccount>()).Returns(_mockPersonalDataBankAccountRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Bank>()).Returns(_mockBankRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<User>()).Returns(_mockUserRepo.Object);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CompleteRSAllowance_ShouldAddClaimHospitalAndSave()
        {
            // Arrange
            var noreg = "12345";
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = noreg
            };

            var reimbursementVm = new ReimbursementViewModel
            {
                FamilyRelationship = "rsanak",
                PatientChildName = "Baby John",
                PatientName = "John Doe",
                Hospital = "RS Example",
                IsOtherHospital = false,
                OtherHospital = "",
                HospitalAddress = "Jl. Contoh No.1",
                DateOfEntry = DateTime.Today.AddDays(-2),
                DateOfOut = DateTime.Today,
                InPatient = "InPatient",
                Cost = 500000,
                TotalClaim = 400000,
                TotalCompanyClaim = 100000,
                AccountName = "John Account",
                AccountNumber = "1234567890",
                BankCode = "BANK01",
                PhoneNumber = "08123456789"
            };

            var documentDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(reimbursementVm)
            };

            _mockDocDetailRepo.Setup(r => r.Fetch())
                              .Returns(new List<DocumentRequestDetail> { documentDetail }.AsQueryable());

            // Act
            _service.CompleteRSAllowance(noreg, documentApproval);

            // Assert
            _mockClaimHospitalRepo.Verify(r => r.Add(It.IsAny<ClaimHospital>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateReimbursementPoint_ShouldUpdateValuesAndSave()
        {
            // Arrange
            var documentApprovalId = Guid.NewGuid();
            var documentApproval = new DocumentApproval
            {
                Id = documentApprovalId,
                DocumentStatusCode = DocumentStatus.InProgress
            };

            var reimbursementVm = new ReimbursementViewModel
            {
                TotalClaim = 500000,
                TotalCompanyClaim = 100000
            };

            var documentDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApprovalId,
                ObjectValue = JsonConvert.SerializeObject(reimbursementVm)
            };

            var point = new ReimbursementPointViewModel
            {
                DocumentApprovalId = documentApprovalId,
                TotalClaim = 600000,
                TotalCompanyClaim = 150000
            };

            _mockDocApprovalRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentApproval> { documentApproval }.AsQueryable());

            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { documentDetail }.AsQueryable());

            // Act
            _service.UpdateReimbursementPoint(point);

            // Assert
            var updatedObj = JsonConvert.DeserializeObject<ReimbursementViewModel>(documentDetail.ObjectValue);
            Assert.Equal(point.TotalClaim, updatedObj.TotalClaim);
            Assert.Equal(point.TotalCompanyClaim, updatedObj.TotalCompanyClaim);
            Assert.True(documentApproval.CanSubmit);

            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
