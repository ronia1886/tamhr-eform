using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle proxy time master data.
    /// </summary>
    public class ProxyTimeService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// User repository object.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// Time management shift repository object.
        /// </summary>
        protected IRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();

        /// <summary>
        /// Time management repository object.
        /// </summary>
        protected IRepository<TimeManagement> TimeManagementRepository => UnitOfWork.GetRepository<TimeManagement>();

        /// <summary>
        /// Time management history repository object.
        /// </summary>
        protected IRepository<TimeManagementHistory> TimeManagementHistoryRepository => UnitOfWork.GetRepository<TimeManagementHistory>();

        /// <summary>
        /// Time management readonly repository object.
        /// </summary>
        protected IReadonlyRepository<TimeManagementView> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagementView>();

        /// <summary>
        /// Daily work schedule readonly repository object.
        /// </summary>
        protected IReadonlyRepository<DailyWorkSchedule> DailyWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<DailyWorkSchedule>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        private readonly string[] _properties = new[] {
            "WorkingTimeIn",
            "WorkingTimeOut",
            "AbsentStatus",
            "Description",
            "ShiftCode",
            "NormalTimeIn",
            "NormalTimeOut",
            "WorkHour"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ProxyTimeService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get time management query
        /// </summary>
        /// <returns>Time Management Query</returns>
        public IQueryable<TimeManagementView> GetQuery()
        {
            return TimeManagementReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get time management query
        /// </summary>
        /// <returns>Time Management Query</returns>
        public IQueryable<TimeManagementView> GetQuery(string noreg, string name, DateTime? startDate, DateTime? endDate)
        {
            if(!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(noreg))
            {
                return GetQuery()
                .Where(x => ((x.Name.Contains(name) || x.NoReg.Contains(noreg)) && (!startDate.HasValue || x.WorkingDate >= startDate.Value) && (!endDate.HasValue || x.WorkingDate <= endDate.Value)) )
                .OrderBy(x => x.Name)
                .ThenBy(x => x.WorkingDate);
            }
            else
            {
                return GetQuery()
                    .Where(x =>  (!startDate.HasValue || x.WorkingDate >= startDate.Value) && (!endDate.HasValue || x.WorkingDate <= endDate.Value))
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.WorkingDate);
            }

        }

        /// <summary>
        /// Get time management history query by date
        /// </summary>
        /// <param name="date">Working Date</param>
        /// <returns>Time Management History Query</returns>
        public IQueryable<TimeManagementHistory> GetHistoriesQuery(string noreg, DateTime date)
        {
            var clearDate = date.Date;

            return TimeManagementHistoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.WorkingDate == date);
        }

        /// <summary>
        /// Get list of time management
        /// </summary>
        /// <returns>List of Time Management</returns>
        public IEnumerable<TimeManagementView> Gets()
        {
            return GetQuery().ToList();
        }

        /// <summary>
        /// Get time management by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Object</returns>
        public TimeManagementView Get(Guid id)
        {
            return GetQuery()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void Upsert(string actor, TimeManagement timeManagement)
        {
            var tm = TimeManagementRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == timeManagement.Id);

            var now = DateTime.Now;
            var listFlexi = new string[] { "1NS8", "1NJ8" };

            var shift = DailyWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ShiftCode == timeManagement.ShiftCode);

            timeManagement.NormalTimeIn = tm.NormalTimeIn;
            timeManagement.NormalTimeOut = tm.NormalTimeOut;


            

            if (shift != null)
            {
                var date = tm.WorkingDate.Date;
                DateTime? normalTimeIn = null;
                DateTime? normalTimeOut = null;

                if (shift.NormalTimeIN.HasValue && shift.NormalTimeOut.HasValue)
                {
                    normalTimeIn = date.Add(shift.NormalTimeIN.Value);
                    normalTimeOut = date.Add(shift.NormalTimeOut.Value);

                    if (shift.NormalTimeOut < shift.NormalTimeIN)
                    {
                        normalTimeOut = normalTimeOut.Value.AddDays(1);
                    }
                }

                timeManagement.NormalTimeIn = normalTimeIn;
                timeManagement.NormalTimeOut = normalTimeOut;
            }



            if (listFlexi.Contains(timeManagement.ShiftCode) && timeManagement.WorkingTimeIn <= timeManagement.NormalTimeIn)
            {
                var difference = timeManagement.WorkingTimeOut - timeManagement.NormalTimeIn;
                timeManagement.WorkHour = (int)difference.Value.TotalMinutes;
            }
            else
            {
                var difference = timeManagement.WorkingTimeOut - timeManagement.WorkingTimeIn;
                timeManagement.WorkHour = difference.HasValue ? (int)difference.Value.TotalMinutes : 0;
            }

            UnitOfWork.Transact((trans) =>
            {
                TimeManagementRepository.Upsert<Guid>(timeManagement, _properties);

                TimeManagementHistoryRepository.Add(TimeManagementHistory.CreateFrom(timeManagement));

                var empWorkSubtitute = EmpWorkSchSubtituteRepository.Fetch().FirstOrDefault(x => x.NoReg == timeManagement.NoReg && x.Date == timeManagement.WorkingDate.Date);

                if (tm.ShiftCode != timeManagement.ShiftCode)
                {
                    if (empWorkSubtitute != null)
                    {
                        empWorkSubtitute.ShiftCodeUpdate = timeManagement.ShiftCode;
                        empWorkSubtitute.CreatedBy = actor;
                        empWorkSubtitute.CreatedOn = now;
                    }
                    else
                    {
                        EmpWorkSchSubtituteRepository.Add(EmpWorkSchSubtitute.CreateFrom(actor, now, tm.ShiftCode, timeManagement));
                    }
                }

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Soft delete time management by id and its dependencies if any
        /// </summary>
        /// <param name="id">Time Management Id</param>
        public void SoftDelete(Guid id)
        {
            var TimeManagement = TimeManagementRepository.FindById(id);

            TimeManagement.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete time management by id and its dependencies if any
        /// </summary>
        /// <param name="id">Time Management Id</param>
        public void Delete(Guid id)
        {
            TimeManagementRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete time management history by id and its dependencies if any
        /// </summary>
        /// <param name="id">Time Management History Id</param>
        public void DeleteHistory(Guid id)
        {
            TimeManagementHistoryRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}
