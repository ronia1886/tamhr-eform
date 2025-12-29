using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle sap general category map master data.
    /// </summary>
    public class SapGeneralCategoryMapService : GenericDomainServiceBase<SapGeneralCategoryMap>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        protected override string[] Properties => new[] { "GeneralCategoryCode", "SapCode", "SapCategory" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public SapGeneralCategoryMapService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
