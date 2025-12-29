using Kendo.Mvc.UI;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using Xunit;
using static TAMHR.ESS.UnitTest.ClaimBenefit.VacationAllowanceServiceTest;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class DailyMonitoringAbsenceServiceTest
    {

        [Fact]
        public void GetDailyMonitoring_ShouldReturn_DataSourceResult()
        {
            // Arrange
            var mockFetcher = new Mock<IDataResultFetcher>();
            var request = new DataSourceRequest();
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            var dummyData = new List<TimeMonitoringSubordinateStoredEntity>
        {
            new TimeMonitoringSubordinateStoredEntity { NoReg = "EMP001", Name = "Dummy User", PostName = "Tester" }
        };

            var expectedResult = new DataSourceResult
            {
                Data = dummyData,
                Total = dummyData.Count
            };

            mockFetcher
                .Setup(x => x.GetDataResult<TimeMonitoringSubordinateStoredEntity>(
                    It.IsAny<string>(),
                    It.IsAny<DataSourceRequest>(),
                    It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var result = mockFetcher.Object.GetDataResult<TimeMonitoringSubordinateStoredEntity>(@"
                    SELECT
				        eo.NoReg,
				        eo.Name,
				        eo.PostName,
				        eo.JobLevel,
				        oa.EmployeeSubgroup,
				        gc.Name AS EmployeeSubgroupText,
                        div.ObjectText AS Division,
                        dep.ObjectText AS Department,
                        sec.ObjectText AS Section,
                        grp.ObjectText AS [Group],
                        lin.ObjectText AS Line,
                        ISNULL(MIN(proxy.ProxyTime),pt.ProxyIn) AS ProxyIn,
                        ISNULL(MAX(proxy.ProxyTime),pt.ProxyOut) AS ProxyOut,
				        CASE WHEN COUNT(pt.ProxyIn)+COUNT(pt.ProxyOut)=0
							THEN SUM(IIF(proxy.Staff_Number IS NULL, 0, 1))
						ELSE COUNT(pt.ProxyIn)+(CASE WHEN pt.ProxyOut IS NOT NULL AND pt.ProxyOut=pt.ProxyIn THEN 0 ELSE 1 END) END AS TotalProxy
			        FROM dbo.SF_GET_EMPLOYEE_BY_ORG(@orgCode, @keyDate) eo
                    LEFT JOIN dbo.MDM_ORGANIZATIONAL_ASSIGNMENT oa ON oa.NoReg = eo.NoReg AND @keyDate BETWEEN oa.StartDate AND oa.EndDate
                    LEFT JOIN dbo.TB_M_GENERAL_CATEGORY gc ON gc.Code = 'es.' + oa.EmployeeSubgroup
			        LEFT JOIN dbo.MDM_POSITION_JOB_REL pjr ON pjr.PostCode = @postCode AND @keyDate BETWEEN pjr.StartDate AND pjr.EndDate
			        LEFT JOIN dbo.MDM_JOB_CHIEF jc ON jc.JobCode = pjr.JobCode
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE div ON div.OrgCode = eo.OrgCode AND div.ObjectDescription = 'Division'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE dep ON dep.OrgCode = eo.OrgCode AND dep.ObjectDescription = 'Department'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE sec ON sec.OrgCode = eo.OrgCode AND sec.ObjectDescription = 'Section'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE grp ON grp.OrgCode = eo.OrgCode AND grp.ObjectDescription = 'Group'
                    LEFT JOIN dbo.MDM_ACTUAL_ENTITY_STRUCTURE lin ON lin.OrgCode = eo.OrgCode AND lin.ObjectDescription = 'Line'
			        LEFT JOIN (
				        SELECT
                            Staff_Number COLLATE DATABASE_DEFAULT AS Staff_Number,
						    CAST(Tr_Date AS DATE) AS ProxyDate,
						    CAST(Tr_Date + ' ' + STUFF(STUFF(Tr_Time, 5, 0, ':'), 3, 0, ':') AS DATETIME) AS ProxyTime
                        FROM OPENQUERY([{0}], 'SELECT * FROM matrix.dbo.Data_Proxy WHERE Staff_Number <> '''' AND Tr_Date = ''{1}''')
                        UNION
                        SELECT
                            Staff_Number COLLATE DATABASE_DEFAULT AS Staff_Number,
						    CAST(Tr_Date AS DATE) AS ProxyDate,
						    CAST(Tr_Date + ' ' + STUFF(STUFF(Tr_Time, 5, 0, ':'), 3, 0, ':') AS DATETIME) AS ProxyTime
                        FROM OPENQUERY([{2}], 'SELECT * FROM Matrix.dbo.Data_Proxy_SPLD WHERE Staff_Number <> '''' AND Tr_Date = ''{1}''')
                        UNION
                        SELECT 
                        NoReg as Staff_Number,
                        AccessDate AS ProxyDate,
                        AccessDateTime As ProxyTime
                        FROM TB_R_ATTENDANCE_RECORD
                        WHERE Noreg <> '' AND AccessDate = @keyDate
			        ) proxy ON proxy.Staff_Number = eo.NoReg
                    LEFT JOIN TB_R_PROXY_TIME pt ON pt.NoReg=eo.NoReg AND CONVERT(varchar(10),pt.WorkingDate,120)=CONVERT(varchar(10), @keyDate, 120)
			        WHERE eo.Staffing = 100 AND (eo.JobLevel > jc.JobLevel OR eo.PostCode = @postCode)
			        GROUP BY eo.NoReg, eo.Name, eo.PostName, eo.JobLevel, oa.EmployeeSubgroup, gc.Name, div.ObjectText, 
                    dep.ObjectText, sec.ObjectText, grp.ObjectText, lin.ObjectText, pt.ProxyIn, pt.ProxyOut
                ", request, new { startDate, endDate });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.IsType<DataSourceResult>(result);
        }
    }
}
