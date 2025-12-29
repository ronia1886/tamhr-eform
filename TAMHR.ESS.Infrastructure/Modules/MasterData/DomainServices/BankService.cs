using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle bank master data.
    /// </summary>
    public class BankService : GenericDomainServiceBase<Bank>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "BankKey",
            "BankName",
            "Branch",
            "City",
            "Address"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public BankService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
