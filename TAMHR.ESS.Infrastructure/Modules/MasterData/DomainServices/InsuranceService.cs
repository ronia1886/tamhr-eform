using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle insurance master data.
    /// </summary>
    public class InsuranceService : GenericDomainServiceBase<PersonalDataInsurance>
    {
        #region Domain Repositories
        /// <summary>
        /// Personal data insurance repository object.
        /// </summary>
        protected IReadonlyRepository<PersonalDataInsuranceView> PersonalDataInsuranceReadonlyRepository => UnitOfWork.GetRepository<PersonalDataInsuranceView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for personal data insurance entity.
        /// </summary>
        protected override string[] Properties => throw new NotImplementedException();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public InsuranceService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IQueryable<PersonalDataInsuranceView> GetInsruances()
        {
            return PersonalDataInsuranceReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.MemberNumber));
        }
        #endregion
    }
}
