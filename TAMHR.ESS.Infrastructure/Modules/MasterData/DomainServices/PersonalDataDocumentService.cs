using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle personal data document master data.
    /// </summary>
    public class PersonalDataDocumentService : GenericDomainServiceBase<PersonalDataDocument>
    {
        #region Domain Repositories
        /// <summary>
        /// User readonly repository object.
        /// </summary>
        protected IReadonlyRepository<User> UserReadonlyRepository => UnitOfWork.GetRepository<User>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for personal data document entity.
        /// </summary>
        protected override string[] Properties => new[] { "DocumentValue" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public PersonalDataDocumentService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods

        public IEnumerable<PersonalDataDocument> GetDocuments()
        {
            return CommonRepository.Fetch().AsEnumerable().ToList();
        }

        public IEnumerable<User> GetActiveUsers()
        {
            return UserReadonlyRepository.Fetch()
                                         .Where(u => u.RowStatus)
                                         .ToList();
        }

        public User GetUserByNoReg(string noReg)
        {
            return UserReadonlyRepository
                .Fetch()
                .FirstOrDefault(u => u.RowStatus && u.NoReg == noReg);
        }

        public override void Upsert(PersonalDataDocument data)
        {
            var isNew = data.Id == Guid.Empty;

            if (isNew)
            {
                data.StartDate = DateTime.Now.Date;
                data.EndDate = new DateTime(9999, 12, 31);
            }

            var personalDocument = CommonRepository.Find(x => x.NoReg == data.NoReg && x.DocumentTypeCode == data.DocumentTypeCode).FirstOrDefault();

            CommonRepository.Attach(personalDocument ?? data, Properties);

            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}
