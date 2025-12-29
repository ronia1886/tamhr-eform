using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Querying;
using Xunit;
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class BonusDeductionServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<AbsenceSummary>> _absenceSummaryRepo;
        private readonly Mock<IRepository<Absence>> _commonRepo;
        private readonly Mock<IServiceProvider> _serviceProvider;
        private readonly Mock<GenericDataQuery> _genericDataQueryMock;

        private readonly AbsenceService _absenceService;

        public BonusDeductionServiceTest()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _unitOfWork = new Mock<IUnitOfWork>();

            _serviceProvider.Setup(s => s.GetService(typeof(IUnitOfWork)))
                        .Returns(_unitOfWork.Object);

            _absenceSummaryRepo = new Mock<IRepository<AbsenceSummary>>();
            _commonRepo = new Mock<IRepository<Absence>>();

            _genericDataQueryMock = new Mock<GenericDataQuery>(_unitOfWork.Object, It.IsAny<DataSourceRequest>())
            {
                CallBase = true
            };

            // inject ke unitofwork
            _unitOfWork.Setup(u => u.GetRepository<AbsenceSummary>()).Returns(_absenceSummaryRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<Absence>()).Returns(_commonRepo.Object);

            _absenceService = new AbsenceService( _unitOfWork.Object );
        }

        [Fact]
        public void GetAbsencesBySummaryCategory_ShouldReturnFilteredDictionary()
        {
            // Arrange
            string category = "bonus-deduction";

            var summaries = new List<AbsenceSummary>
            {
                new AbsenceSummary { Id = Guid.NewGuid(), PresenceCode = 1, SummaryCategoryCode = category, RowStatus = true },
                new AbsenceSummary { Id = Guid.NewGuid(), PresenceCode = 2, SummaryCategoryCode = category, RowStatus = true },
                new AbsenceSummary { Id = Guid.NewGuid(), PresenceCode = 3, SummaryCategoryCode = "other", RowStatus = true },
            }.AsQueryable();

            var absences = new List<Absence>
            {
                new Absence { CodePresensi = 1, Name = "Bonus" },
                new Absence { CodePresensi = 2, Name = "Deduction" },
                new Absence { CodePresensi = 99, Name = "Other" }
            }.AsQueryable();

            _absenceSummaryRepo.Setup(r => r.Fetch()).Returns(summaries);
            _commonRepo.Setup(r => r.Fetch()).Returns(absences);

            // Act
            var result = _absenceService.GetAbsencesBySummaryCategory(category);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal("Bonus", result[1]);
            Assert.Equal("Deduction", result[2]);
        }
    }
}
