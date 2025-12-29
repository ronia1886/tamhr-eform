using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle BPJS master data
    /// </summary>
    public class BpjsService : GenericDomainServiceBase<PersonalDataBpjs>
    {
        #region Domain Repositories
        /// <summary>
        /// Personal data BPJS readonly repository object.
        /// </summary>
        protected IReadonlyRepository<PersonalDataBpjsView> PersonalDataBpjsReadonlyRepository => UnitOfWork.GetRepository<PersonalDataBpjsView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for personal data BPJS entity.
        /// </summary>
        protected override string[] Properties => new[] { "" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public BpjsService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of BPJS.
        /// </summary>
        /// <returns>This list of <see cref="PersonalDataBpjsView"/> objects.</returns>
        public IQueryable<PersonalDataBpjsView> GetBpjs()
        {
            // Get list of BPJS objects.
            return PersonalDataBpjsReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.BpjsNumber));
        }
        #endregion
    }
}
