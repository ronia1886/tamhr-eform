using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class ScpServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Vehicle>> _mockVehicleRepo;
        private readonly Mock<IRepository<VehicleMatrix>> _mockVehicleMatrixRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _mockOrgRepo;
        private readonly Mock<IRepository<DocumentApproval>> _mockDocumentApproval;
        private readonly Mock<IRepository<Form>> _mockForm;
        private readonly Mock<IRepository<FormValidationMatrix>> _mockFormValidation;
        private readonly Mock<IRepository<CarPurchase>> _mockCarPurchase;
        private readonly Mock<IStringLocalizer<IUnitOfWork>> _mockLocalizer;
        private readonly ClaimBenefitService _service;

        public ScpServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();

            _mockVehicleRepo = new Mock<IRepository<Vehicle>>();
            _mockVehicleMatrixRepo = new Mock<IRepository<VehicleMatrix>>();
            _mockOrgRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _mockDocumentApproval = new Mock<IRepository<DocumentApproval>>();
            _mockForm = new Mock<IRepository<Form>>();
            _mockFormValidation = new Mock<IRepository<FormValidationMatrix>>();
            _mockCarPurchase = new Mock<IRepository<CarPurchase>>();

            _mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();

            // Setup localizer
            _mockLocalizer.Setup(l => l["SCP Request must be completed before create request"])
                          .Returns(new LocalizedString(
                              "SCP Request must be completed before create request",
                              "SCP Request must be completed before create request"
                          ));

            _unitOfWork.Setup(u => u.GetRepository<Vehicle>()).Returns(_mockVehicleRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<VehicleMatrix>()).Returns(_mockVehicleMatrixRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_mockOrgRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_mockDocumentApproval.Object);
            _unitOfWork.Setup(u => u.GetRepository<Form>()).Returns(_mockForm.Object);
            _unitOfWork.Setup(u => u.GetRepository<FormValidationMatrix>()).Returns(_mockFormValidation.Object);
            _unitOfWork.Setup(u => u.GetRepository<CarPurchase>()).Returns(_mockCarPurchase.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateScp()
        {
            // Arrange
            string noreg = "123";
            string name = "EMP001";

            _mockVehicleRepo.Setup(r => r.Fetch()).Returns(new List<Vehicle>().AsQueryable());
            _mockVehicleMatrixRepo.Setup(r => r.Fetch()).Returns(new List<VehicleMatrix>().AsQueryable());
            _mockOrgRepo.Setup(r => r.Fetch()).Returns(new List<ActualOrganizationStructure>().AsQueryable());

            // Act
            var ex = Assert.Throws<Exception>(() => _service.PreValidateScp(noreg, name));

            // Assert
            Assert.Contains("no vehicle to purchase for this class", ex.Message);
        }

        [Fact]
        public void PreValidateScps()
        {
            // Arrange
            string noreg = "EMP001";

            var form = new Form { Id = Guid.NewGuid(), FormKey = "scp" };
            var docApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                CreatedBy = noreg,
                RowStatus = true,
                DocumentStatusCode = "draft"
            };

            var mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();
            mockLocalizer.Setup(l => l["SCP Request must be completed before create request"])
                         .Returns(new LocalizedString(
                             "SCP Request must be completed before create request",
                             "SCP Request must be completed before create request"
                         ));

            // Set field private menggunakan reflection
            var field = typeof(ClaimBenefitService)
                            .GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_service, mockLocalizer.Object);

            _mockForm.Setup(r => r.Fetch()).Returns(new List<Form> { form }.AsQueryable());
            _mockDocumentApproval.Setup(r => r.Fetch()).Returns(new List<DocumentApproval> { docApproval }.AsQueryable());

            // Act
            var ex = Assert.Throws<Exception>(() => _service.PreValidateScp(noreg));

            // Assert
            Assert.Contains("SCP Request must be completed", ex.Message);
        }

        [Fact]
        public void PreValidatePostScp()
        {
            // Arrange
            var noreg = "12345";
            var formKey = "scp";
            var requestDetail = new DocumentRequestDetailViewModel<ScpViewModel>
            {
                FormKey = formKey
            };

            // Siapkan data matrix
            var validateMatrix = new Form
            {
                Id = Guid.NewGuid(),
                FormKey = formKey,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(5)
            };
            _mockFormValidation.Setup(r => r.Fetch())
                .Returns(new List<FormValidationMatrix>
                {
            new FormValidationMatrix { FormId = validateMatrix.Id, RequestType = "scp" }
                }.AsQueryable());

            _mockForm.Setup(r => r.Fetch())
                .Returns(new List<Form> { validateMatrix }.AsQueryable());

            _mockCarPurchase.Setup(r => r.Find(It.IsAny<Expression<Func<CarPurchase, bool>>>()))
            .Returns(new List<CarPurchase>
            {
                new CarPurchase { NoReg = noreg, CarPurchaseType = "SCP", CreatedOn = DateTime.Today }
            });

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidatePostScp(noreg, requestDetail));
            Assert.Contains("Cannot create request until", ex.Message);
        }

        [Fact]
        public void CompleteSCPAllowance_ShouldAddCarPurchaseAndSave()
        {
            // Arrange
            var noreg = "12345";
            var documentApproval = new DocumentApproval { Id = Guid.NewGuid(), CreatedBy = noreg };

            var scpVm = new ScpViewModel
            {
                PurchaseTypeCode = "Type1",
                Model = "ModelX",
                TypeName = "Sedan",
                ColorCode = "Red",
                PopulationNumber = "NIK123",
                DTRRN = "DateTime.Today",
                DTMOCD = "DateTime.Today",
                DTMOSX = "DateTime.Today",
                DTEXTC = "DateTime.Today",
                DTPLOD = "DateTime.Today",
                DTFRNO = "DateTime.Today",
                Dealer = "Dealer1",
                DoDate = DateTime.Today,
                StnkDate = DateTime.Today,
                Jasa = 1000,
                PaymentMethod = "Cash"
            };

            var documentDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(scpVm)
            };

            var mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            mockDocDetailRepo.Setup(r => r.Fetch()).Returns(new List<DocumentRequestDetail> { documentDetail }.AsQueryable());

            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(mockDocDetailRepo.Object);

            // Service instance
            var service = new ClaimBenefitService(_unitOfWork.Object);

            // UnitOfWork.SaveChanges mock
            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            service.CompleteSCPAllowance(noreg, documentApproval);

            // Assert
            _mockCarPurchase.Verify(r => r.Add(It.IsAny<CarPurchase>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
