using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Querying;
using Xunit;
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class VacationAllowanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<DocumentApproval>> _mockDocApprovalRepo;
        private readonly Mock<IRepository<DocumentRequestDetail>> _mockDocDetailRepo;
        private readonly Mock<IRepository<VacationAllowanceViewModel>> _mockVacationViewModel;
        private readonly Mock<IRepository<RecreationReward>> _mockRecreationReward;
        private readonly ClaimBenefitService _claimBenefitService;

        public VacationAllowanceServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockDocApprovalRepo = new Mock<IRepository<DocumentApproval>>();
            _mockDocDetailRepo = new Mock<IRepository<DocumentRequestDetail>>();
            _mockRecreationReward = new Mock<IRepository<RecreationReward>>();
            _mockVacationViewModel = new Mock<IRepository<VacationAllowanceViewModel>>();

            // Setup repositories
            _unitOfWork.Setup(u => u.GetRepository<DocumentApproval>())
                       .Returns(_mockDocApprovalRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<DocumentRequestDetail>())
                       .Returns(_mockDocDetailRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<VacationAllowanceViewModel>())
                       .Returns(_mockVacationViewModel.Object);
            _unitOfWork.Setup(u => u.GetRepository<RecreationReward>())
                       .Returns(_mockRecreationReward.Object);

            // Inject ke service
            _claimBenefitService = new ClaimBenefitService(_unitOfWork.Object);
        }

        [Fact]
        public void CompleteVacationAllowance_ShouldUpdate_RecreationReward_AndSaveChanges()
        {
            // Arrange
            var service = new Mock<IVacationServiceTest>();

            var documentApproval = new DocumentApproval
            {
                Id = Guid.NewGuid(),
                CreatedBy = "EMP001"
            };

            var vacationDate = new DateTime(2025, 10, 1);
            var reward = new RecreationReward
            {
                NoReg = "EMP001",
                EventDate = vacationDate,
                DocumentApprovalId = Guid.Empty
            };

            service.Setup(s => s.CompleteVacationAllowance("EMP001", documentApproval))
                   .Callback(() =>
                   {
                       reward.DocumentApprovalId = documentApproval.Id;
                       _unitOfWork.Object.SaveChanges();
                   });

            // Act
            service.Object.CompleteVacationAllowance("EMP001", documentApproval);

            // Assert
            Assert.Equal(documentApproval.Id, reward.DocumentApprovalId);
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

            var dummyData = new List<RecreationReward>
        {
            new RecreationReward { Id = Guid.NewGuid(), Name = "Dummy User", PostName = "Tester" }
        };

            var expectedResult = new DataSourceResult
            {
                Data = dummyData,
                Total = dummyData.Count
            };

            mockFetcher
                .Setup(x => x.GetDataResult<RecreationReward>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<RecreationReward>("query", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }

        public interface IDataResultFetcher
        {
            DataSourceResult GetDataResult<T>(string query, DataSourceRequest request, object parameters) where T : class;
        }

        public interface IVacationServiceTest
        {
            public void CompleteVacationAllowance(string noregCurentApprover, DocumentApproval documentApproval);
        }
    }
}
