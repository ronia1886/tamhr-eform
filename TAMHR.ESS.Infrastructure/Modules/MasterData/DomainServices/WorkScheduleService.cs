using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System.Linq;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle work schedule master data.
    /// </summary>
    public class WorkScheduleService : GenericDomainServiceBase<WorkSchedule>
    {
        #region Domain Repositories
        /// <summary>
        /// Work schedule rule readonly repository object.
        /// </summary>
        protected IReadonlyRepository<WorkScheduleRule> WorkScheduleRuleReadonlyRepository => UnitOfWork.GetRepository<WorkScheduleRule>();

        protected IRepository<WorkSchedule> WorkScheduleRepository { get { return UnitOfWork.GetRepository<WorkSchedule>(); } }
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        protected override string[] Properties => throw new NotImplementedException();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public WorkScheduleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IEnumerable<WorkScheduleRule> GetWorkingScheduleRules()
        {
            return WorkScheduleRuleReadonlyRepository.Fetch().AsNoTracking();
        }
        public IQueryable<WorkSchedule> GetLatestWorkSchedules()
        {
            return WorkScheduleRepository.Fetch().AsNoTracking();
        }
        #endregion
    }
}
