using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle allowance detail master data.
    /// </summary>
    public class AllowanceDetailService : GenericDomainServiceBase<AllowanceDetail>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for allowance detail entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Type",
            "SubType",
            "ClassFrom",
            "ClassTo",
            "Ammount",
            "Description",
            "StartDate",
            "EndDate"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public AllowanceDetailService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
