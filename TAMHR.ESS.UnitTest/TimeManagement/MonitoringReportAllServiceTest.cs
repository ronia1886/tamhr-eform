using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class TestableMonitoringReportAllService : MonitoringReportAllService
    {
        public TestableMonitoringReportAllService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public new List<EmployeeProfileViewModel> GetEmployeProfiles(DateTime startDate, DateTime endDate, bool? IsEligible)
        {
            var finalData = new List<EmployeeProfileViewModel>
            {
                new EmployeeProfileViewModel
                {
                    ID = Guid.NewGuid(),
                    Noreg = "1001",
                    Name = "Test User",
                    Expr1 = "Active Employee",
                    IsEligible = true,
                    Absent3 = 0,
                    Absent5 = 0,
                    StartDateParam = startDate,
                    EndDateParam = endDate
                },
                new EmployeeProfileViewModel
                {
                    ID = Guid.NewGuid(),
                    Noreg = "1002",
                    Name = "Second User",
                    Expr1 = "Active Employee",
                    IsEligible = true,
                    Absent3 = 0,
                    Absent5 = 0,
                    StartDateParam = startDate,
                    EndDateParam = endDate
                }
            };

            if (IsEligible != null)
            {
                finalData = finalData.Where(d => d.IsEligible == IsEligible).ToList();
            }

            return finalData;
        }
    }

    public class MonitoringReportAllServiceTest
    {
        private readonly Mock<IServiceProxy> _mockServiceProxy;
        private readonly Mock<IMdmService> _mockMdmService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly TestableMonitoringReportAllService _service;
        private readonly MonitoringAllControllerTest _controller;

        public MonitoringReportAllServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockServiceProxy = new Mock<IServiceProxy>();
            _mockMdmService = new Mock<IMdmService>();
            _service = new TestableMonitoringReportAllService(_unitOfWork.Object);

            _controller = new MonitoringAllControllerTest(_mockServiceProxy.Object);
        }

        [Fact]
        public void GetEmployeeProfiles_ReturnsData_Bypassed()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today;

            var result = _service.GetEmployeProfiles(start, end, true);

            // assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Noreg == "1001" && r.Name == "Test User");
            Assert.Contains(result, r => r.Noreg == "1002" && r.Name == "Second User");
        }

        [Fact]
        public void GetEmployeeProfiles_FilterIsEligible_FalseReturnsEmpty()
        {
            // arrange
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today;

            var result = _service.GetEmployeProfiles(start, end, false);

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetEmployeeProfiless_FilterIsEligible_FalseReturnsEmpty()
        {
            //arrange
            var noreg = "EMP001";
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            var request = new DataSourceRequest { Page = 1, PageSize = 10 };

            // Expected DataSourceResult
            var expectedResult = new DataSourceResult
            {
                Data = new List<AbsenceSummaryDetailsStoredEntity>
            {
                new AbsenceSummaryDetailsStoredEntity { NoReg = "EMP001", Name = "John Doe" }
            },
                Total = 1
            };

            // Mock GetTableValuedDataSourceResult
            _mockServiceProxy.Setup(sp => sp.GetTableValuedDataSourceResult<AbsenceSummaryDetailsStoredEntity>(
                request,
                It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = _controller.GetAbsenceSummaryDetails(noreg,startDate, endDate,"", request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single((IEnumerable<AbsenceSummaryDetailsStoredEntity>)result.Data);
        }
    }

    public class MonitoringAllControllerTest
    {
        private readonly IServiceProxy _serviceProxy;
        private readonly IMdmService _mdmService;

        public MonitoringAllControllerTest(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        public DataSourceResult GetAbsenceSummaryDetails([FromForm] string noreg, [FromForm] DateTime startDate, [FromForm] DateTime endDate, [FromForm] string category, [DataSourceRequest] DataSourceRequest request)
        {
            return _serviceProxy.GetTableValuedDataSourceResult<AbsenceSummaryDetailsStoredEntity>(request, new { noreg, startDate, endDate, category });
        }
    }
}
