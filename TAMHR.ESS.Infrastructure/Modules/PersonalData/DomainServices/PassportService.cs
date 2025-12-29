using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class PassportService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Passport repository
        /// </summary>
        protected IRepository<PersonalDataPassport> PassportRepository => UnitOfWork.GetRepository<PersonalDataPassport>();


        #endregion

        #region Private Properties
        private readonly string[] _properties = new[] {
            "PassportNumber",
            "NoReg",
            "Type",
            "CountryCode",
            "Office",
            "StartDate",
            "EndDate"
        };
        #endregion

        #region Constructor
        public PassportService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// Get form by id
        /// </summary>
        /// <param name="id">Form id</param>
        /// <returns>Form Object</returns>
        public PersonalDataPassport Get(Guid id)
        {
            return PassportRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert form.
        /// </summary>
        /// <param name="actor">This actor.</param>
        /// <param name="form">This <see cref="Form"/> input object.</param>
        public void Upsert(PassportViewModel form)
        {
            var list = new PersonalDataPassport
            {
                PassportNumber = form.PassportNumber,
                NoReg = form.NoReg,
                CountryCode = form.CountryCode,
                Office = form.Office,
                StartDate = form.StartDate,
                EndDate = form.EndDate
            };
            PassportRepository.Add(list);

            UnitOfWork.SaveChanges();
        }

        ///// <summary>
        ///// Soft delete form by id and its dependencies if any
        ///// </summary>
        ///// <param name="id">Form Id</param>
        //public void SoftDelete(Guid id)
        //{
        //    var form = FormRepository.FindById(id);

        //    form.RowStatus = false;

        //    UnitOfWork.SaveChanges();
        //}

        ///// <summary>
        ///// Delete form by id and its dependencies if any
        ///// </summary>
        ///// <param name="id">Form Id</param>
        //public void Delete(Guid id)
        //{
        //    FormRepository.DeleteById(id);

        //    UnitOfWork.SaveChanges();
        //}

        ///// <summary>
        ///// Get ISO number by form key.
        ///// </summary>
        ///// <param name="formkey">This form key.</param>
        ///// <returns>This ISO number.</returns>
        //public string GetIsoNumber(string formkey)
        //{
        //    var form = FormRepository.Fetch()
        //        .AsNoTracking()
        //        .Where(x => x.FormKey == formkey)
        //        .FirstOrDefaultIfEmpty();

        //    return form.IsoNumber;
        //}

        /// <summary>
        /// Get list of form questions by form key.
        /// </summary>
        /// <param name="formKey">This form key.</param>
        /// <returns>This list of <see cref="FormQuestion"/> objects.</returns>
        public IEnumerable<FormQuestion> GetFormQuestions(string formKey, bool IsActive = true)
        {
            var formQuestionSet = UnitOfWork.GetRepository<FormQuestion>();

            var formQuestions = formQuestionSet.Fetch()
                .AsNoTracking()
                .Include(x => x.Form)
                .Where(x => x.Form.FormKey == formKey && x.IsActive == IsActive)
                .OrderBy(x => x.OrderSequence);

            return formQuestions;
        }

        public IEnumerable<FormQuestion> GetFormQuestions(IEnumerable<Guid> ids)
        {
            var formQuestionSet = UnitOfWork.GetRepository<FormQuestion>();

            var formQuestions = formQuestionSet.Fetch()
                .AsNoTracking()
                .Include(x => x.ParentFormQuestion)
                .Where(x => ids.Contains(x.Id))
                .ToList()
                .OrderBy(x => x.ParentFormQuestion?.OrderSequence)
                .ThenBy(x => x.OrderSequence);

            return formQuestions;
        }

        public IEnumerable<FormQuestionDetail> GetFormQuestionDetails(Guid formQuestionId)
        {
            var formQuestionSet = UnitOfWork.GetRepository<FormQuestionDetail>();

            var formQuestions = formQuestionSet.Fetch()
                .AsNoTracking()
                .Where(x => x.FormQuestionId == formQuestionId)
                .ToList()
                .OrderBy(x => x.OrderSequence);

            return formQuestions;
        }




        #endregion
    }
}
