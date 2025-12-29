using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
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
    public class KBAllowanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<KBAllowance>> _mockKbAllowanceRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly ClaimBenefitService _service; // ganti sesuai service yang digunakan

        public KBAllowanceServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockKbAllowanceRepo = new Mock<IRepository<KBAllowance>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();

            _unitOfWork.Setup(u => u.GetRepository<KBAllowance>()).Returns(_mockKbAllowanceRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>()).Returns(_mockDocDetailRepo.Object);

            _service = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CompleteKBAllowance_ShouldAddKBAllowance_AndSaveChanges()
        {
            // Arrange
            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001",
                CreatedOn = DateTime.Now
            };

            var kbVm = new KbAllowanceViewModel
            {
                FamilyRelationship = "Spouse",
                PassienName = "Jane Doe",
                Hospital = "Hospital A",
                HospitalAddress = "Street 123",
                ActionKBDate = DateTime.Now,
                Cost = 1500000
            };

            var docDetail = new DocumentRequestDetail
            {
                DocumentApprovalId = documentApproval.Id,
                ObjectValue = JsonConvert.SerializeObject(kbVm)
            };

            _mockDocDetailRepo.Setup(r => r.Fetch())
                .Returns(new List<DocumentRequestDetail> { docDetail }.AsQueryable());

            // Act
            _service.CompleteKBAllowance(documentApproval.CreatedBy, documentApproval);

            // Assert
            _mockKbAllowanceRepo.Verify(r => r.Add(It.Is<KBAllowance>(k =>
                k.NoReg == documentApproval.CreatedBy &&
                k.FamilyMemberType == kbVm.FamilyRelationship &&
                k.PatientName == kbVm.PassienName &&
                k.HospitalName == kbVm.Hospital &&
                k.HospitalAddress == kbVm.HospitalAddress &&
                k.TransactionDate == kbVm.ActionKBDate.Value &&
                k.Ammount == kbVm.Cost
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
