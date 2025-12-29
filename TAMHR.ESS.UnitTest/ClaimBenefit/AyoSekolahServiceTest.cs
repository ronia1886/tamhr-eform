using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;
using static TAMHR.ESS.UnitTest.ClaimBenefit.VacationAllowanceServiceTest;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class AyoSekolahServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Form>> _formRepository;
        private readonly Mock<IRepository<DocumentApproval>> _documentApprovalRepository;
        private readonly Mock<IStringLocalizer> _localizer;
        private readonly Mock<IStringLocalizer<IUnitOfWork>> _mockLocalizer;
        private readonly ClaimBenefitService _service;

        public AyoSekolahServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _formRepository = new Mock<IRepository<Form>>();
            _documentApprovalRepository = new Mock<IRepository<DocumentApproval>>();
            _localizer = new Mock<IStringLocalizer>();
            _mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();

            // Setup localizer
            _mockLocalizer.Setup(l => l["SCP Request must be completed before create request"])
                          .Returns(new LocalizedString(
                              "SCP Request must be completed before create request",
                              "SCP Request must be completed before create request"
                          ));

            _unitOfWork.Setup(x => x.GetRepository<Form>()).Returns(_formRepository.Object);
            _unitOfWork.Setup(x => x.GetRepository<DocumentApproval>()).Returns(_documentApprovalRepository.Object);

            _service = new ClaimBenefitService(
                _unitOfWork.Object
            );
        }

        [Fact]
        public void PreValidateAyoSekolah_ShouldThrowException_WhenDocumentAlreadyExists()
        {
            // Arrange
            var noreg = "10001";
            var form = new Form
            {
                Id = Guid.NewGuid(),
                FormKey = ApplicationForm.AyoSekolah,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            _formRepository.Setup(x => x.Fetch())
                .Returns(new List<Form> { form }.AsQueryable());

            _documentApprovalRepository.Setup(x => x.Fetch())
                .Returns(new List<DocumentApproval>
                {
                new DocumentApproval
                {
                    CreatedBy = noreg,
                    FormId = form.Id,
                    CreatedOn = DateTime.Now,
                    DocumentStatusCode = DocumentStatus.Draft
                }
                }.AsQueryable());

            var mockLocalizer = new Mock<IStringLocalizer<IUnitOfWork>>();
            mockLocalizer.Setup(l => l["Cannot create new request because Document is Already Created"])
                         .Returns(new LocalizedString(
                             "Cannot create new request because Document is Already Created",
                             "Cannot create new request because Document is Already Created"
                         ));

            // Set field private menggunakan reflection
            var field = typeof(ClaimBenefitService)
                            .GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_service, mockLocalizer.Object);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.PreValidateAyoSekolah(noreg));
            Assert.Equal("Cannot create new request because Document is Already Created", ex.Message);
        }

        [Fact]
        public void PreValidateAyoSekolah_ShouldPass_WhenNoExistingDocument()
        {
            // Arrange
            var noreg = "10002";
            var form = new Form
            {
                Id = Guid.NewGuid(),
                FormKey = ApplicationForm.AyoSekolah,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            _formRepository.Setup(x => x.Fetch())
                .Returns(new List<Form> { form }.AsQueryable());

            _documentApprovalRepository.Setup(x => x.Fetch())
                .Returns(new List<DocumentApproval>().AsQueryable());

            // Act & Assert (tidak ada exception)
            _service.PreValidateAyoSekolah(noreg);
        }

        [Fact]
        public void GetDetailsReport_ShouldReturn_DataSourceResult()
        {
            // Arrange
            var mockFetcher = new Mock<IDataResultFetcher>();
            var request = new DataSourceRequest();
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            var dummyData = new List<AyoSekolah>
        {
            new AyoSekolah { Id = Guid.NewGuid(), Name = "Dummy User", PostName = "Tester" }
        };

            var expectedResult = new DataSourceResult
            {
                Data = dummyData,
                Total = dummyData.Count
            };

            mockFetcher
                .Setup(x => x.GetDataResult<AyoSekolah>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<AyoSekolah>(
                @"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_AYO_SEKOLAH i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg 
                WHERE i.CreatedOn BETWEEN @startDate AND @endDate", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
