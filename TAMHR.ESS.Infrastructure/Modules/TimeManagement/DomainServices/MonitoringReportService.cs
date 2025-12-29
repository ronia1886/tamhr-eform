using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class MonitoringReportService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Time management readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagementView> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagementView>();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public MonitoringReportService(IUnitOfWork unitOfWork)
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
            var summary = UnitOfWork.UspQuery<TimeManagementSummaryStoredEntity>(new { noreg,postCode, orgCode, startDate, endDate }).FirstOrDefaultIfEmpty();

            return summary;
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

        public DataSourceResult GetDailyMonitoring(DataSourceRequest request, DateTime keyDate, string orgCode, string postCode)
        {
            var data = UnitOfWork
            .UdfQuery<TimeMonitoringSubordinateStoredEntity>(new { postCode, orgCode, keyDate })
            .OrderBy(x => x.Division);

            return data.ToDataSourceResult(request);
        }
    }
}
