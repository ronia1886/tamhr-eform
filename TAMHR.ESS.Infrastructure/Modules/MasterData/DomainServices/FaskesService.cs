using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle faskes master data.
    /// </summary>
    public class FaskesService : GenericDomainServiceBase<Faskes>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for faskes entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "FaskesCode",
            "FaskesName",
            "FaskesAddress",
            "FaskesCity"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public FaskesService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
