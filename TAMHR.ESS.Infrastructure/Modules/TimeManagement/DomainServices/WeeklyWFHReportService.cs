//using TAMHR.ESS.Infrastructure;
using Agit.Common.Attributes;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.TimeManagement;
using static System.Collections.Specialized.BitVector32;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class WeeklyWFHReportService : DomainServiceBase
    {
        public WeeklyWFHReportService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        public IEnumerable<WeeklyWFHStoredEntity> GetWeeklyWFHReport(DateTime StartDate, DateTime EndDate, string NoReg = null, string PostCode = null)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<UnitOfWork>(), UnitOfWork.GetConnection(), sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)).Options;
            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();
            DatabaseObjectAttribute databaseObjectAttribute = DatabaseObjectAttribute.Get<WeeklyWFHStoredEntity>(DatabaseObjectType.StoredProcedure);

            return unitOfWork.GetConnection().Query<WeeklyWFHStoredEntity>(databaseObjectAttribute.Name, new { StartDate = StartDate, EndDate = EndDate, NoReg = NoReg, PostCode = PostCode }, null, buffered: true, connectionTimeout.Value, CommandType.StoredProcedure).ToList();
        }

        public IEnumerable<WeeklyWFHSummaryStoredEntity> GetWeeklyWFHSummaryReport(DateTime StartDate, DateTime EndDate, bool firstLoad = false, string NoReg = null, string PostCode = null)
        {

            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<UnitOfWork>(), UnitOfWork.GetConnection(), sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)).Options;
            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();
            DatabaseObjectAttribute databaseObjectAttribute = DatabaseObjectAttribute.Get<WeeklyWFHSummaryStoredEntity>(DatabaseObjectType.StoredProcedure);

            return unitOfWork.GetConnection().Query<WeeklyWFHSummaryStoredEntity>(databaseObjectAttribute.Name, new { StartDate = StartDate, EndDate = EndDate, FirstLoad = firstLoad, NoReg = NoReg, PostCode = PostCode }, null, buffered: true, connectionTimeout.Value, CommandType.StoredProcedure).ToList();

        }

        public IEnumerable<dynamic> GetWeeklyWFHSummaryPlanReport(string field, string category, DateTime StartDate, DateTime EndDate, bool firstLoad = false, string NoReg = null, string PostCode = null, string filter = null)
        {

            var filterQuery = string.IsNullOrEmpty(filter) ? "1=1" : filter;
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<UnitOfWork>(), UnitOfWork.GetConnection(), sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)).Options;
            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();

            var innerQuery = string.Format(@"SELECT * FROM TB_T_SUMMARY_WEEKLY_WFH_REPORT WHERE ({0})", filterQuery);
            var outerQuery = string.Format(@"SELECT * FROM dbo.TB_M_GENERAL_CATEGORY WHERE Category = '{0}'", category);
            var bodyQuery = string.Format(@"SELECT cat.Code, cat.Name, ISNULL(COUNT(src.{2}), 0) AS Total FROM ({0}) cat LEFT JOIN ({1}) src ON src.{2} = cat.Code Where cat.Description like '%True%'  GROUP BY cat.Code, cat.Name", outerQuery, innerQuery, field);
            return unitOfWork.GetConnection().Query(bodyQuery, null, null, buffered: true, connectionTimeout.Value);
        }

        public IEnumerable<WeeklyWFHDownloadStoredEntity> GetWeeklyWFHDownloadReport(DateTime StartDate, DateTime EndDate, string NoReg = null, string PostCode = null)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<UnitOfWork>(), UnitOfWork.GetConnection(), sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)).Options;
            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();
            DatabaseObjectAttribute databaseObjectAttribute = DatabaseObjectAttribute.Get<WeeklyWFHDownloadStoredEntity>(DatabaseObjectType.StoredProcedure);
            return unitOfWork.GetConnection().Query<WeeklyWFHDownloadStoredEntity>(databaseObjectAttribute.Name, new { StartDate = StartDate, EndDate = EndDate, NoReg = NoReg, PostCode = PostCode }, null, buffered: true, connectionTimeout.Value, CommandType.StoredProcedure).ToList();
        }

        public IEnumerable<WeeklyWFHDownloadStoredEntity> GetWeeklyWFHDownloadReports(DateTime StartDate, DateTime EndDate, string NoReg = null, string PostCode = null)
        {
            return UnitOfWork.UdfQuery<WeeklyWFHDownloadStoredEntity>(new { StartDate = StartDate, EndDate = EndDate, NoReg = NoReg, PostCode = PostCode });
        }

        public IEnumerable<WeeklyWFHChartSummary> GetWeeklyWFHChartsReport(
     string MonthYear,
     string employeeName,
     string Directorate,
     string Division,
     string Department,
     string Section,
     string Line,
     string Group,
     string ClassName,
     string PlanWorkPlace,
     string ActualWorkPlace,
     string noreg,
     string FirstLoad)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder<UnitOfWork>(),
                UnitOfWork.GetConnection(),
                sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)
            ).Options;

            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();

            string storedProcedureName = "[dbo].[SP_MONTHLY_CHARTS_REPORTS]";

            return unitOfWork.GetConnection().Query<WeeklyWFHChartSummary>(
                storedProcedureName,
                new
                {
                    MonthYear,
                    employeeName,
                    Directorate,
                    Division,
                    Department,
                    Section,
                    Line,
                    Group,
                    ClassName,
                    PlanWorkPlace,
                    ActualWorkPlace,
                    noreg,
                    FirstLoad
                },
                commandTimeout: connectionTimeout,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public IEnumerable<GeneralCategory> GetWeeklyWFHChartsReportSimplify(
     string MonthYear,
     string employeeName,
     string Directorate,
     string Division,
     string Department,
     string Section,
     string Line,
     string Group,
     string ClassName,
     string PlanWorkPlace,
     string ActualWorkPlace,
     string noreg,
     string postCode,
     string FirstLoad,
     string NoRegSuperior,
     string PostCodeSuperior)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder<UnitOfWork>(),
                UnitOfWork.GetConnection(),
                sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)
            ).Options;

            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();

            string storedProcedureName = "[dbo].[SP_MONTHLY_CHARTS_REPORTS_SIMPLIFY]";
            MonthYear = MonthYear.Split(' ')[2] + '-' + MonthYear.Split(' ')[1] + '-' + MonthYear.Split(' ')[0];
            return unitOfWork.GetConnection().Query<GeneralCategory>(
                storedProcedureName,
                new
                {
                    MonthYear,
                    employeeName,
                    Directorate,
                    Division,
                    Department,
                    Section,
                    Line,
                    Group,
                    ClassName,
                    PlanWorkPlace,
                    ActualWorkPlace,
                    noreg,
                    postCode,
                    FirstLoad,
                    NoRegSuperior,
                    PostCodeSuperior
                },
                commandTimeout: connectionTimeout,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public IEnumerable<WeeklyWFHChartSummary> GetWeeklyWFHChartsReportSuperior(
     string MonthYear,
     string employeeName,
     string Directorate,
     string Division,
     string Department,
     string Section,
     string Line,
     string Group,
     string ClassName,
     string PlanWorkPlace,
     string ActualWorkPlace,
     string noreg,
     string FirstLoad)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder<UnitOfWork>(),
                UnitOfWork.GetConnection(),
                sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)
            ).Options;

            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();

            string storedProcedureName = "[dbo].[SP_MONTHLY_CHARTS_REPORTS_SUPERIOR]";

            return unitOfWork.GetConnection().Query<WeeklyWFHChartSummary>(
                storedProcedureName,
                new
                {
                    MonthYear,
                    employeeName,
                    Directorate,
                    Division,
                    Department,
                    Section,
                    Line,
                    Group,
                    ClassName,
                    PlanWorkPlace,
                    ActualWorkPlace,
                    noreg,
                    FirstLoad
                },
                commandTimeout: connectionTimeout,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public IEnumerable<WeeklyWFHChartSummary> GetWeeklyWFHChartsReportSuperiorSimplify(
     string MonthYear,
     string employeeName,
     string Directorate,
     string Division,
     string Department,
     string Section,
     string Line,
     string Group,
     string ClassName,
     string PlanWorkPlace,
     string ActualWorkPlace,
     string noreg,
     string FirstLoad)
        {
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder<UnitOfWork>(),
                UnitOfWork.GetConnection(),
                sqlServerOptions => sqlServerOptions.CommandTimeout(UnitOfWork.GetConnection().ConnectionTimeout)
            ).Options;

            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();

            string storedProcedureName = "[dbo].[SP_MONTHLY_CHARTS_REPORTS_SUPERIOR_SIMPLIFY]";

            return unitOfWork.GetConnection().Query<WeeklyWFHChartSummary>(
                storedProcedureName,
                new
                {
                    MonthYear,
                    employeeName,
                    Directorate,
                    Division,
                    Department,
                    Section,
                    Line,
                    Group,
                    ClassName,
                    PlanWorkPlace,
                    ActualWorkPlace,
                    noreg,
                    FirstLoad
                },
                commandTimeout: connectionTimeout,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public IEnumerable<DailyReconciliationSummaryStoredEntity> GetDailyReconciliationSummary(DateTime? DateFrom, DateTime? DateTo, string category = "absence", bool showAll = false)
        {
            return UnitOfWork.UdfQuery<DailyReconciliationSummaryStoredEntity>(new { startDate = DateFrom, endDate = DateTo, category, showAll });
        }

        public IQueryable<OrganizationStructureView> GetDirectorates()
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Directorate");
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDivisions()
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDivisionsFromMultiDirectorate(List<string> DirectorateList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division" && DirectorateList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDepartments(string Division)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Department" && x.ParentOrgCode == Division);
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDepartmentsFromMultiDivision(List<string> DivisionList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Department" && DivisionList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetSections(string Department)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section" && x.ParentOrgCode == Department);
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetSectionsFromMultiDepartment(List<string> DepartmentList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section" && DepartmentList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetLinesFromMultiSection(List<string> SectionList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Line" && SectionList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetGroupsFromMultiLine(List<string> LineList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Group" && LineList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public List<Vaccine> GetClass()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT NK_SubKelas as Class from vw_personal_data_information Order By NK_SubKelas
            ").ToList();
        }

        public List<Absence> GetPresenceCode()
        {
            return UnitOfWork.GetConnection().Query<Absence>(@"
            select * FROM TB_M_ABSENT
            ").ToList();
        }

        public IEnumerable<GeneralCategory> GetGeneralCategories(string category)
        {
            return UnitOfWork.GetRepository<GeneralCategory>()
                .Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category)
                .ToList();
        }

        public async Task<List<WeeklyWFHPlanningDetail>> GetEventsAsync(string noreg, bool isChief)
        {
            var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var parameters = new DynamicParameters();
            parameters.Add("@StartDates", startDate.ToString("yyyy-MM-dd"), DbType.Date);
            parameters.Add("@EndDates", endDate.ToString("yyyy-MM-dd"), DbType.Date);

            // Define the stored procedure and parameters based on chief status
            string storedProcedure = isChief
                ? "[dbo].[SP_MONTHLY_REPORT_SUPERIOR]"
                : "[dbo].[SP_MONTHLY_REPORT]";

            if (isChief)
            {
                // Add @NoReg parameter for the superior stored procedure
                parameters.Add("@NoReg", noreg, DbType.String);
            }

            using (var connection = UnitOfWork.GetConnection())
            {
                try
                {
                    var result = await connection.QueryAsync<WeeklyWFHPlanningDetail>(
                        $"EXEC {storedProcedure} @StartDates, @EndDates{(isChief ? ", @NoReg" : "")}",
                        parameters
                    );

                    var orderedResults = result
                        .OrderBy(r => r.NoReg != null && r.NoReg == noreg ? 0 : 1)
                        .ThenBy(r => r.NoReg)
                        .ToList();

                    return orderedResults;
                }
                catch (Exception ex)
                {
                    // Replace with your logging framework
                    Console.Error.WriteLine($"Error retrieving events: {ex.Message}");
                    throw; // Rethrow to propagate the exception
                }
            }
        }


        public async Task<List<WeeklyWFHPlanningDetail>> GetEventsSuperiorAsync(string noreg)
        {

            var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var startDateString = startDate.ToString("yyyy-MM-dd");

            // Calculate EndDate as the last day of the current month
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var endDateString = endDate.ToString("yyyy-MM-dd");

            var parameters = new DynamicParameters();
            parameters.Add("@StartDate", startDateString, DbType.Date);
            parameters.Add("@EndDate", endDateString, DbType.Date);
            parameters.Add("@NoReg", noreg, DbType.String);

            using (var connection = UnitOfWork.GetConnection())
            {
                try
                {
                    // Execute the stored procedure asynchronously
                    var result = await connection.QueryAsync<WeeklyWFHPlanningDetail>(@"
                EXEC [dbo].[SP_MONTHLY_REPORT_SUPERIOR] @StartDate, @EndDate, @NoReg
            ", parameters);

                    // Order the results by NoReg, with current NoReg first
                    var orderedResults = result.OrderBy(r => r.NoReg != null && r.NoReg == noreg ? 0 : 1)
                                                  .ThenBy(r => r.NoReg)
                                                  .ToList();

                    return orderedResults;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving events: {ex.Message}");
                    throw; // Rethrow the exception to propagate it
                }
            }
        }

        public async Task<List<WeeklyWFHPlanningDetail>> GetFiltersAsync(
     string startDate,
     string employeeName,
     string Directorate,
     string Division,
     string Department,
     string Section,
     string Line,
     string Group,
     string ClassName,
     string PlanWorkPlace,
     string ActualWorkPlace)
        {
            try
            {
                // Parsing month-year to a standard format
                string monthYear = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(startDate, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        monthYear = parsedDate.ToString("MMMM yyyy");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid startDate format. Expected format is 'MMMM yyyy'.");
                    }
                }

                // Preparing parameters
                var parameters = new DynamicParameters();
                parameters.Add("@StartDates", DBNull.Value, DbType.Date);
                parameters.Add("@EndDates", DBNull.Value, DbType.Date);
                parameters.Add("@MonthYear", monthYear ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@EmployeeName", employeeName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Directorate", Directorate ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Division", Division ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Department", Department ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Section", Section ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Line", Line ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Group", Group ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ClassName", ClassName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@PlanWorkPlace", PlanWorkPlace ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ActualWorkPlace", ActualWorkPlace ?? (object)DBNull.Value, DbType.String);

                using (var connection = UnitOfWork.GetConnection())
                {
                    var result = await connection.QueryAsync<WeeklyWFHPlanningDetail>(@"
                EXEC [dbo].[SP_MONTHLY_REPORT] 
                    @StartDates, 
                    @EndDates,
                    @MonthYear, 
                    @EmployeeName, 
                    @Directorate, 
                    @Division, 
                    @Department, 
                    @Section, 
                    @Line, 
                    @Group,
                    @ClassName,  
                    @PlanWorkPlace, 
                    @ActualWorkPlace
            ", parameters);

                  
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving filtered data: {ex.Message}");
                throw; // Rethrow the exception to propagate it
            }
        }

        public async Task<List<WeeklyWFHPlanningDetail>> GetDataReport(string startDate,string employeeName,string Directorate,string Division,string Department,
            string Section,string Line,string Group,string ClassName,string PlanWorkPlace,string ActualWorkPlace, string NoRegSuperior, string PostCodeSuperior, string NoReg, string PostCode)
        {
            try
            {
                // Parsing month-year to a standard format
                string monthYear = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(startDate, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        monthYear = parsedDate.ToString("MMMM yyyy");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid startDate format. Expected format is 'MMMM yyyy'.");
                    }
                }

                // Preparing parameters
                var parameters = new DynamicParameters();
                
                parameters.Add("@MonthYear", monthYear ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@EmployeeName", employeeName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Directorate", Directorate ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Division", Division ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Department", Department ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Section", Section ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Line", Line ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Group", Group ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ClassName", ClassName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@PlanWorkPlace", PlanWorkPlace ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ActualWorkPlace", ActualWorkPlace ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@NoRegSuperior", NoRegSuperior ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@PostCodeSuperior", PostCodeSuperior ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@NoReg", NoReg?? (object)DBNull.Value, DbType.String);
                parameters.Add("@PostCode", PostCode?? (object)DBNull.Value, DbType.String);

                using (var connection = UnitOfWork.GetConnection())
                {
                    var result = await connection.QueryAsync<WeeklyWFHPlanningDetail>(@"
                    EXEC [dbo].[SP_MONTHLY_REPORT_SIMPLIFY] 
                        @MonthYear, 
                        @EmployeeName, 
                        @Directorate, 
                        @Division, 
                        @Department, 
                        @Section, 
                        @Line, 
                        @Group,
                        @ClassName,  
                        @PlanWorkPlace, 
                        @ActualWorkPlace,
                        @NoRegSuperior,
                        @PostCodeSuperior,
                        @NoReg,
                        @PostCode
                    ", parameters);

                    var cek = result.Where(wh => wh.Date.ToString("yyyy-MM-dd") == "2025-02-03" && wh.Plan == "WFH").ToList();
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving filtered data: {ex.Message}");
                throw; // Rethrow the exception to propagate it
            }
        }

    public async Task<List<WeeklyWFHPlanningDetail>> GetFiltersSuperiorAsync(
    string noreg,
    string startDate,
    string employeeName,
    string Directorate,
    string Division,
    string Department,
    string Section,
    string Line,
    string Group,
    string ClassName,
    string PlanWorkPlace,
    string ActualWorkPlace)
        {
            try
            {
                // Parsing month-year to a standard format
                string monthYear = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(startDate, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        monthYear = parsedDate.ToString("MMMM yyyy");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid startDate format. Expected format is 'MMMM yyyy'.");
                    }
                }

                // Preparing parameters
                var parameters = new DynamicParameters();
                parameters.Add("@NoReg", noreg ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@MonthYear", monthYear ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@EmployeeName", employeeName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Directorate", Directorate ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Division", Division ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Department", Department ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Section", Section ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Line", Line ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@Group", Group ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ClassName", ClassName ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@PlanWorkPlace", PlanWorkPlace ?? (object)DBNull.Value, DbType.String);
                parameters.Add("@ActualWorkPlace", ActualWorkPlace ?? (object)DBNull.Value, DbType.String);

                using (var connection = UnitOfWork.GetConnection())
                {
                    var result = await connection.QueryAsync<WeeklyWFHPlanningDetail>(@"
                EXEC [dbo].[SP_MONTHLY_FILTER_REPORT_SUPERIOR] 
                    @NoReg,
                    @MonthYear, 
                    @EmployeeName, 
                    @Directorate, 
                    @Division, 
                    @Department, 
                    @Section, 
                    @Line,
                    @Group,
                    @ClassName,  
                    @PlanWorkPlace,
                    @ActualWorkPlace
            ", parameters);


                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving filtered data: {ex.Message}");
                throw; // Rethrow the exception to propagate it
            }
        }


    }
}