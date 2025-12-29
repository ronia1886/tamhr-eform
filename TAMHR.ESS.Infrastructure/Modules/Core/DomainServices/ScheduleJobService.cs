using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class ScheduleJobService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Sync log repository
        /// </summary>
        protected IRepository<ScheduleJob> scheduleJobRepository => UnitOfWork.GetRepository<ScheduleJob>();
        #endregion
        #region constructor
        public ScheduleJobService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        #endregion

        public async Task<Guid> CreateScheduleJob(string jobName)
        {
            var newSchedule = new ScheduleJob()
            {
                JobName = jobName,
                Start = DateTime.Now,
                CreatedOn = DateTime.Now,
                CreatedBy = "system",
                RowStatus = true
            };
             
            scheduleJobRepository.Add(newSchedule);
            await UnitOfWork.SaveChangesAsync();

            return newSchedule.Id;
        }

        public async Task FinishScheduleJob(Guid id)
        {
            var existingSchedule = scheduleJobRepository.FindById(id);
            if(existingSchedule == null)
            {
                return;
            }

            existingSchedule.Finish = DateTime.Now;
            await UnitOfWork.SaveChangesAsync();
           
        }
    }
}
