using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Azure.Core;
using Dapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;
using static System.Collections.Specialized.BitVector32;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class MonitoringReportAllService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Time management readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagementView> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagementView>();
        protected IRepository<EmployeProfileView> EmployeeProfileViewRepository => UnitOfWork.GetRepository<EmployeProfileView>();
        protected IRepository<TimeManagement> TimeManagementRepository => UnitOfWork.GetRepository<TimeManagement>();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public MonitoringReportAllService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        /// <summary>
        /// Get time management monitoring
        /// </summary>
        /// <returns>List of Time Managements</returns>
        public IQueryable<TimeManagementView> Gets()
        {
            return TimeManagementReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get time monitoring summary
        /// </summary>
        /// <param name="postCode">Position Code</param>
        /// <param name="orgCode">Organization Code</param>
        /// <param name="keyDate">Key Date</param>
        /// <param name="checkMonth">Check Month</param>
        /// <returns>Time Monitoring Summary Object</returns>
        public TimeManagementSummaryStoredEntity GetSummary(string noreg, string postCode, string orgCode, DateTime startDate, DateTime endDate)
        {
            var summary = UnitOfWork.UspQuery<TimeManagementSummaryStoredEntity>(new { noreg, postCode, orgCode, startDate, endDate }).FirstOrDefaultIfEmpty();

            return summary;
        }
        public IEnumerable<string> GetInvitationUniqueColumnValuesDashboard(string fieldName)
        {
            
            var attribute = typeof(EmployeProfileView).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

            if (fieldName == "Expr1")
            {
                string sql1 = $"SELECT DISTINCT {fieldName} FROM {attribute.Name} WHERE {fieldName} IS NOT NULL AND { fieldName} = 'Active Employee' ";

                return UnitOfWork.GetConnection().Query<string>(sql1);
            }
            //var attribute = typeof(EmployeProfileActualView).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            string sql = $"SELECT DISTINCT {fieldName} FROM {attribute.Name} WHERE {fieldName} IS NOT NULL";

            return UnitOfWork.GetConnection().Query<string>(sql);
        }

        /// <summary>
        /// Get list of proxy details by noreg and key date
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="keyDate">Key Date</param>
        /// <returns>List of Proxy Details</returns>
        public IEnumerable<ProxyDetailsStoredEntity> GetMatrixProxyDetails(string noreg, DateTime keyDate)
        {
            var keyDateStr = keyDate.ToString("yyyyMMdd");

            return UnitOfWork.UspQuery<ProxyDetailsStoredEntity>(new { noreg, keyDateStr });
        }

        public List<EmployeeProfileViewModel> GetEmployeProfiles(DateTime startDate, DateTime endDate, bool? IsEligible)
        {
            string sqlQuery = @"
			SELECT NoReg, MIN(IsEligible) as IsEligible FROM (
			SELECT tm_sub.AbsentStatus,tm_sub.NoReg,gc.Code,(CASE WHEN COUNT(*)<=CAST(gc.Name as INT) THEN 1 ELSE 0 END) as isEligible FROM TB_R_TIME_MANAGEMENT tm_sub 
			INNER JOIN tb_m_general_category gc ON REPLACE(gc.Code,'Eligible_','')=tm_sub.AbsentStatus AND gc.category='IncentiveEligibleCriteria'
			WHERE WorkingDate >= @startDate AND WorkingDate <= @endDate
			GROUP BY tm_sub.AbsentStatus,tm_sub.NoReg,gc.Code,gc.Name
			) t_sub1 GROUP BY NoReg
			";

            var listDataEligible = UnitOfWork.GetConnection().Query<EmployeeProfileViewModel>(sqlQuery, new { startDate, endDate });
            //var excludedEmployeeSubgroupText = new List<string> { "3", "4", "5", "6"};
            //var data = EmployeeProfileViewRepository.Fetch().AsNoTracking().Where(wh => wh.NP<=7).ToList();
            //var data = EmployeeProfileViewRepository.Fetch().AsNoTracking().Where(wh => wh.NP<7 && wh.Expr1 == "Active Employee").ToList();
            var data = EmployeeProfileViewRepository.Fetch().AsNoTracking().Where(wh => wh.NP<7 && wh.Expr1 == "Active Employee" ).ToList();

            //string[] arrAbsentStatus = { "11", "28","34","36","37" };

            string sqlQuery2 = @"
			SELECT NoReg
            ,COUNT(CASE WHEN AbsentStatus=3 THEN 1 END) as Absent3
            ,COUNT(CASE WHEN AbsentStatus=5 THEN 1 END) as Absent5            
            ,COUNT(CASE WHEN AbsentStatus=11 THEN 1 END) as Absent11 
            ,COUNT(CASE WHEN AbsentStatus=28 THEN 1 END) as Absent28 
            ,COUNT(CASE WHEN AbsentStatus=34 THEN 1 END) as Absent34 
            ,COUNT(CASE WHEN AbsentStatus=35 THEN 1 END) as Absent35 
            ,COUNT(CASE WHEN AbsentStatus=36 THEN 1 END) as Absent36 
            ,COUNT(CASE WHEN AbsentStatus=37 THEN 1 END) as Absent37 
            from TB_R_TIME_MANAGEMENT tm WHERE tm.WorkingDate BETWEEN @startDate AND @endDate
            GROUP BY NoReg
			";
            var listAbsent = UnitOfWork.GetConnection().Query<EmployeeProfileViewModel>(sqlQuery2, new { startDate, endDate });

            //var dataTimeManagement = TimeManagementRepository.Fetch().AsNoTracking().Where(wh => wh.WorkingDate>=startDate && wh.WorkingDate<=endDate && arrAbsentStatus.Contains(wh.AbsentStatus)).ToList();
            
            var final_data = (from t1 in data
                              join t2 in listDataEligible on t1.Noreg equals t2.Noreg into gj
                              join t3 in listAbsent on t1.Noreg equals t3.Noreg into gj2
                              from subq1 in gj.DefaultIfEmpty()
                              from subq2 in gj2.DefaultIfEmpty()
                              select new EmployeeProfileViewModel
                             {
                                 ID = t1.ID,
                                 OrgCode = t1.OrgCode,
                                 ParentOrgCode = t1.ParentOrgCode,
                                 OrgName = t1.OrgName,
                                 Service = t1.Service,
                                 Noreg = t1.Noreg,
                                 Name = t1.Name,
                                 PostCode = t1.PostCode,
                                 PostName = t1.PostName,
                                 JobName = t1.JobName,
                                 EmployeeGroup = t1.EmployeeGroup,
                                 Expr1 = t1.Expr1,
                                 EmployeeSubGroup = t1.EmployeeSubGroup,
                                 EmployeeSubGroupText = t1.EmployeeSubGroupText,
                                 WorkContractText = t1.WorkContractText,
                                 PersonalArea = t1.PersonalArea,
                                 PersonalSubArea = t1.PersonalSubArea,
                                 DepthLevel = t1.DepthLevel,
                                 Staffing = t1.Staffing,
                                 Chief = t1.Chief,
                                 Period = t1.Period,
                                 Vacant = t1.Vacant,
                                 Structure = t1.Structure,
                                 Divisi = t1.Divisi,
                                 Department = t1.Department,
                                 Section = t1.Section,
                                 Directorate = t1.Directorate,
                                 DirOrgCode = t1.DirOrgCode,
                                 DivOrgCode = t1.DivOrgCode,
                                 DepOrgCode = t1.DepOrgCode,
                                 SecOrgCode = t1.SecOrgCode,
                                 IsEligible = subq1 == null?true: subq1.IsEligible,
                                 Absent3 = subq2 == null ? 0 : subq2.Absent3,
                                 Absent5 = subq2 == null ? 0 : subq2.Absent5,
                                 Absent11 = subq2 == null ? 0 : subq2.Absent11,
                                 Absent28 = subq2 == null ? 0 : subq2.Absent28,
                                 Absent34 = subq2 == null ? 0 : subq2.Absent34,
                                 Absent35 = subq2 == null ? 0 : subq2.Absent35,
                                 Absent36 = subq2 == null ? 0 : subq2.Absent36,
                                 Absent37 = subq2 == null ? 0 : subq2.Absent37,
                                 StartDateParam = startDate,
                                 EndDateParam = endDate
                              }).ToList();
            if (IsEligible!=null)
            {
                final_data = final_data.Where(c => c.IsEligible == IsEligible).ToList();
            }

            return final_data;

        }

        public IEnumerable<string> getClass()
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NP<7 && x.Expr1 == "Active Employee")
                .Select(x => x.EmployeeSubGroupText).Distinct().ToList();

            return data;
        }

        public IEnumerable<string> getCategory()
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NP < 7 && x.Expr1 == "Active Employee")
                .Select(x => x.WorkContractText).Distinct().ToList();

            return data;
        }

        public DataSourceResult GetDailyMonitoring(DataSourceRequest request, string keyDate, string orgCode, string postCode)
        {
            var data = UnitOfWork.UdfQuery<TimeMonitoringSubordinateStoredEntity>(new { postCode, orgCode, keyDate });
            return data.ToDataSourceResult(request);
        }
    }
}
