using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle hospital master data.
    /// </summary>
    public class HospitalService : GenericDomainServiceBase<Hospital>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for hospital entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Name",
            "Province",
            "City",
            "Address"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public HospitalService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
