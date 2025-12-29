using System;
using System.Linq;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle daily work schedule master data.
    /// </summary>
    public class DailyWorkScheduleService : GenericDomainServiceBase<DailyWorkSchedule>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for daily work schedule entity.
        /// </summary>
        protected override string[] Properties => new[] { "ColorClass" }; 
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public DailyWorkScheduleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IEnumerable<DailyWorkSchedule> GetDailyWorkSchedules(IEnumerable<string> shiftCodes)
        {
            return Gets().Where(x => shiftCodes.Contains(x.ShiftCode));
        }

        public bool Contains(string shiftCode)
        {
            return Gets().Any(x => x.ShiftCode == shiftCode);
        }
        #endregion
    }
}
