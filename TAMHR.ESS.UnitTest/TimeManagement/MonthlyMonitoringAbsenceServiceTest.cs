using Kendo.Mvc;
using Kendo.Mvc.UI;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class MonthlyMonitoringAbsenceServiceTest
    {
        private readonly Mock<IServiceProxy> _mockServiceProxy;
        private readonly Mock<IMdmService> _mockMdmService;
        private readonly MonitoringControllerTest _controller;

        public MonthlyMonitoringAbsenceServiceTest()
        {
            _mockServiceProxy = new Mock<IServiceProxy>();
            _mockMdmService = new Mock<IMdmService>();

            _controller = new MonitoringControllerTest(_mockServiceProxy.Object, _mockMdmService.Object);
        }

        [Fact]
        public void GetMonthlymonitoring_ShouldReturn_DataSourceResult()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            var request = new DataSourceRequest { Page = 1, PageSize = 10 };

            var noreg = "EMP001";
            var postCode = "PC01";
            var orgCode = "ORG123";

            _mockServiceProxy.Setup(sp => sp.UserClaim)
                .Returns(new UserClaim { NoReg = noreg, PostCode = postCode, Chief = false });

            _mockMdmService.Setup(m => m.GetActualOrganizationStructure(noreg, postCode))
                .Returns(new Organization { OrgCode = orgCode });

            // Expected DataSourceResult
            var expectedResult = new DataSourceResult
            {
                Data = new List<TimeMonitoringSubordinateMonthlyStoredEntity>
            {
                new TimeMonitoringSubordinateMonthlyStoredEntity { NoReg = "EMP001", Name = "John Doe" }
            },
                Total = 1
            };

            // Mock GetTableValuedDataSourceResult
            _mockServiceProxy.Setup(sp => sp.GetTableValuedDataSourceResult<TimeMonitoringSubordinateMonthlyStoredEntity>(
                request,
                It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = _controller.GetMonthlymonitoring(startDate, endDate, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single((IEnumerable<TimeMonitoringSubordinateMonthlyStoredEntity>)result.Data);
        }
    }

    public interface IServiceProxy
    {
        UserClaim UserClaim { get; }
        DataSourceResult GetTableValuedDataSourceResult<T>(DataSourceRequest request, object objectParameters) where T : class;
    }

    public interface IMdmService
    {
        Organization GetActualOrganizationStructure(string noreg, string postCode);
    }

    public class UserClaim
    {
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public bool Chief { get; set; }
    }

    public class Organization
    {
        public string OrgCode { get; set; }
    }

    public class MonitoringControllerTest
    {
        private readonly IServiceProxy _serviceProxy;
        private readonly IMdmService _mdmService;

        public MonitoringControllerTest(IServiceProxy serviceProxy, IMdmService mdmService)
        {
            _serviceProxy = serviceProxy;
            _mdmService = mdmService;
        }

        public DataSourceResult GetMonthlymonitoring(DateTime startDate, DateTime endDate, DataSourceRequest request)
        {
            var noreg = _serviceProxy.UserClaim.NoReg;
            var postCode = _serviceProxy.UserClaim.PostCode;
            var organization = _mdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organization.OrgCode;

            if (!_serviceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }

            return _serviceProxy.GetTableValuedDataSourceResult<TimeMonitoringSubordinateMonthlyStoredEntity>(
                request, new { postCode, orgCode, startDate, endDate });
        }
    }

}
