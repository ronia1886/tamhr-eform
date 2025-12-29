using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle BDJK master data.
    /// </summary>
    public class BdjkDetailService : GenericDomainServiceBase<BdjkDetail>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for BDJK detail entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "BdjkCode",
            "Description",
            "BdjkValue",
            "ClassFrom",
            "ClassTo",
            "FlagHoliday",
            "FlagDuration"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public BdjkDetailService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
