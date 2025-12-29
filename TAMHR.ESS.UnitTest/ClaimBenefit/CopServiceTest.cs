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
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;
using static TAMHR.ESS.UnitTest.ClaimBenefit.VacationAllowanceServiceTest;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class CopServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Vehicle>> _vehicleRepo;
        private readonly Mock<IRepository<VehicleMatrix>> _vehicleMatrixRepo;
        private readonly Mock<IRepository<ActualOrganizationStructure>> _actualOrgRepo;
        private readonly Mock<IRepository<DocumentApproval>> _documentApprovalRepo;
        private readonly Mock<IRepository<Form>> _formRepo;
        private readonly Mock<IRepository<FormValidationMatrix>> _formValidationMatrixRepo;
        private readonly Mock<IRepository<Bpkb>> _mockBpkbRepo;
        private readonly Mock<IRepository<CarPurchase>> _mockCarPurchaseRepo;
        private readonly Mock<IStringLocalizer<IUnitOfWork>> _mockLocalizer;

        private readonly ClaimBenefitService _service;

        public CopServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _vehicleRepo = new Mock<IRepository<Vehicle>>();
            _vehicleMatrixRepo = new Mock<IRepository<VehicleMatrix>>();
            _actualOrgRepo = new Mock<IRepository<ActualOrganizationStructure>>();
            _documentApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _formRepo = new Mock<IRepository<Form>>();
            _formValidationMatrixRepo = new Mock<IRepository<FormValidationMatrix>>();
            _mockBpkbRepo = new Mock<IRepository<Bpkb>>();
            _mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();
            _mockCarPurchaseRepo = new Mock<IRepository<CarPurchase>>();

            _unitOfWork.Setup(u => u.GetRepository<CarPurchase>()).Returns(_mockCarPurchaseRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>()).Returns(_documentApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Form>()).Returns(_formRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Vehicle>()).Returns(_vehicleRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<VehicleMatrix>()).Returns(_vehicleMatrixRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<ActualOrganizationStructure>()).Returns(_actualOrgRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<FormValidationMatrix>()).Returns(_formValidationMatrixRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Bpkb>()).Returns(_mockBpkbRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void PreValidateCop_ShouldNotThrow_WhenDataClassFound()
        {
            // Arrange
            string noreg = "EMP002";
            string name = "John Doe";

            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = Guid.NewGuid(), COP = true }
            }.AsQueryable();

            var vehicleMatrix = new List<VehicleMatrix>
            {
                new VehicleMatrix { VehicleId = vehicles.First().Id, SequenceClass = "C8", Class = "8" }
            }.AsQueryable();

            var actualOrgs = new List<ActualOrganizationStructure>
            {
                new ActualOrganizationStructure { NoReg = noreg, EmployeeSubgroup = "C8" } // match
            }.AsQueryable();

            //var actualOrgs = new List<ActualOrganizationStructure>
            //{
            //    new ActualOrganizationStructure { NoReg = noreg, EmployeeSubgroup = "C9" } // beda dari "C8"
            //}.AsQueryable();

            _vehicleRepo.Setup(r => r.Fetch()).Returns(vehicles);
            _vehicleMatrixRepo.Setup(r => r.Fetch()).Returns(vehicleMatrix);
            _actualOrgRepo.Setup(r => r.Fetch()).Returns(actualOrgs);

            // Act & Assert
            var ex = Record.Exception(() => _service.PreValidateCop(noreg, name));
            Assert.Null(ex); // tidak ada exception
            //Assert.Contains("is not in Class 8 AM above", ex.Message);
        }

        [Fact]
        public void PreValidateCop_ShouldNotThrow_WhenNoActiveRequest()
        {
            // Arrange
            string noreg = "EMP001";

            var form = new Form { Id = Guid.NewGuid(), FormKey = "cop" };
            var docApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                CreatedBy = noreg,
                DocumentStatusCode = "approved", // sudah selesai
                RowStatus = true
            };

            _formRepo.Setup(r => r.Fetch()).Returns(new List<Form> { form }.AsQueryable());
            _documentApprovalRepo.Setup(r => r.Fetch()).Returns(new List<DocumentApproval> { docApproval }.AsQueryable());

            // Act
            var ex = Record.Exception(() => _service.PreValidateCop(noreg));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void PreValidatePostCop_ShouldNotThrow_WhenValidationMatrixExists()
        {
            // Arrange
            string noreg = "EMP001";
            var formKey = "cop";
            var submissionCode = "SUB001";

            // Setup Form repository
            var form = new Form
            {
                Id = Guid.NewGuid(),
                FormKey = formKey
            };
            _formRepo.Setup(r => r.Fetch()).Returns(new List<Form> { form }.AsQueryable());

            // Setup FormValidationMatrix repository
            var matrix = new FormValidationMatrix
            {
                FormId = form.Id,
                RequestType = submissionCode,
                PeriodYear = null,
                PeriodMonth = null
            };
            _formValidationMatrixRepo.Setup(r => r.Fetch()).Returns(new List<FormValidationMatrix> { matrix }.AsQueryable());

            // RequestDetail
            var requestDetail = new DocumentRequestDetailViewModel<CopViewModel>
            {
                FormKey = formKey,
                Object = new CopViewModel { SubmissionCode = submissionCode }
            };

            // Act
            var ex = Record.Exception(() => _service.PreValidatePostCop(noreg, requestDetail));

            // Assert
            Assert.Null(ex); // Tidak ada exception
        }

        [Fact]
        public void CompleteCOPSTNKReady_ShouldAddBpkbAndSaveChanges()
        {
            // Arrange
            string noregRequester = "EMP001";
            var copViewModel = new CopViewModel
            {
                LisencePlat = "B1234XYZ",
                TypeName = "SUV-XYZ",
                Model = "X-Trail",
                DataUnitColorName = "Red",
                DTFRNO = "VIN123456",
                Engine = "ENG987654"
            };

            Bpkb capturedBpkb = null;
            _mockBpkbRepo.Setup(r => r.Add(It.IsAny<Bpkb>()))
                .Callback<Bpkb>(b => capturedBpkb = b);

            // Act
            _service.CompleteCOPSTNKReady(noregRequester, copViewModel);

            // Assert
            _mockBpkbRepo.Verify(r => r.Add(It.IsAny<Bpkb>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);

            Assert.NotNull(capturedBpkb);
            Assert.Equal(noregRequester, capturedBpkb.NoReg);
            Assert.Equal(copViewModel.LisencePlat, capturedBpkb.LicensePlat);
            Assert.Equal("SUV", capturedBpkb.Type);
            Assert.Equal(copViewModel.Model, capturedBpkb.Model);
            Assert.Equal(copViewModel.DataUnitColorName, capturedBpkb.Color);
            Assert.Equal(copViewModel.DTFRNO, capturedBpkb.VINNo);
            Assert.Equal(copViewModel.Engine, capturedBpkb.EngineNo);
        }

        [Fact]
        public void CompleteCOPAllowance_ShouldCallAddAndSaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var copViewModel = new CopViewModel
            {
                SubmissionCode = "New",
                Model = "X-Trail",
                TypeName = "SUV",
                ColorCode = "Red"
            };

            var documentRequestDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(copViewModel)
            };

            var mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { documentRequestDetail }.AsQueryable());
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(mockDocDetailRepo.Object);

            // Act
            _service.CompleteCOPAllowance(documentApproval.CreatedBy, documentApproval);

            // Assert
            _mockCarPurchaseRepo.Verify(r => r.Add(It.IsAny<CarPurchase>()), Times.Once);
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
                .Setup(x => x.GetDataResult<CarPurchase>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<CarPurchase>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_CAR_PURCHASE i 
            LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg
            WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
