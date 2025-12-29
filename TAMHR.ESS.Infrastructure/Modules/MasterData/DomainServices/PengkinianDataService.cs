using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using System.Collections.Generic;
using Agit.Domain.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle pengkinian data master data.
    /// </summary>
    public class PengkinianDataService : GenericDomainServiceBase<PersonalDataFamilyMember>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for pengkinian data entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "NoReg",
            "CommonAttributeId",
            "FamilyTypeCode",
            "StartDate",
            "EndDate"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public PengkinianDataService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IEnumerable<PersonalDataAllStoredEntity> GetPersonalDataByUser(string noreg)
        {
            return UnitOfWork.UdfQuery<PersonalDataAllStoredEntity>(new { noreg });
        }
        #endregion
    }
}
