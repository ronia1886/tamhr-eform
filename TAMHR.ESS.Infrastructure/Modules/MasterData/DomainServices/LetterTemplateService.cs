using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle letter template master data.
    /// </summary>
    public class LetterTemplateService : GenericDomainServiceBase<LetterTemplate>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for letter template entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Title",
            "LetterKey",
            "LetterContent",
            "Description"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public LetterTemplateService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
    }
}
