using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle online letter
    /// </summary>
    public class OnlineLetterService : GenericDomainServiceBase<OnlineLetter>
    {
        #region Domain Repositories
        /// <summary>
        /// Online letter readonly repository.
        /// </summary>
        protected IReadonlyRepository<OnlineLetterView> OnlineLetterReadonlyRepository => UnitOfWork.GetRepository<OnlineLetterView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// List of properties to update.
        /// </summary>
        protected override string[] Properties => new[] {
            "LetterDate",
            "Department",
            "PicTarget",
            "CompanyTarget",
            "LetterTypeCode",
            "Remarks"
        }; 
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public OnlineLetterService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of online letters.
        /// </summary>
        /// <returns>This list of <see cref="OnlineLetterView"/> objects.</returns>
        public IQueryable<OnlineLetterView> GetView()
        {
            return OnlineLetterReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get online letter by id.
        /// </summary>
        /// <param name="id">This online letter id.</param>
        /// <returns>This <see cref="OnlineLetterView"/> object.</returns>
        public OnlineLetterView GetViewById(Guid id)
        {
            return GetView().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Override default update or insert data.
        /// </summary>
        /// <param name="data">This <see cref="OnlineLetter"/> input object.</param>
        public override void Upsert(OnlineLetter data)
        {
            UnitOfWork.Transact(() =>
            {
                var latestId = GetLatestId() + 1;

                if (data.Id == Guid.Empty)
                {
                    data.LetterNumber = latestId;
                }

                base.Upsert(data);
            });
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get latest online letter number.
        /// </summary>
        /// <returns></returns>
        private int GetLatestId()
        {
            var currentYear = DateTime.Now.Year;

            return CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.LetterDate.Value.Year == currentYear)
                .Max(x => (int?)x.LetterNumber) ?? 0;
        }
        #endregion
    }
}
