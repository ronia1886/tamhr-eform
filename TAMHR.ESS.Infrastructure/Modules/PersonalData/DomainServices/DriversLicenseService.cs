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
    public class DriversLicenseService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Drivers License repository
        /// </summary>
        protected IRepository<PersonalDataDriverLicense> DriversLicenseRepository => UnitOfWork.GetRepository<PersonalDataDriverLicense>();

       
        #endregion

        #region Private Properties
        private readonly string[] _properties = new[] {
            "SimType",
            "SimNumber",
            "StartDate",
            "EndDate"
        };
        #endregion

        #region Constructor
        public DriversLicenseService(IUnitOfWork unitOfWork)
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
        public PersonalDataDriverLicense Get(Guid id)
        {
            return DriversLicenseRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert form.
        /// </summary>
        /// <param name="actor">This actor.</param>
        /// <param name="form">This <see cref="Form"/> input object.</param>
        //public void Upsert(DriverLicenseViewModel form)
        //{
        //        var list = new PersonalDataDriverLicense
        //        {
        //            SimType = form.SimType,
        //            SimNumber = form.SimNumber,
        //            StartDate = form.StartDate,
        //            EndDate = form.EndDate
        //        };
        //    DriversLicenseRepository.Add(list);

        //    UnitOfWork.SaveChanges();
        //}
        public bool SaveDriverLicense(DriverLicenseViewModel entity)
        {

            var list = new PersonalDataDriverLicense
            {
                SimNumber = entity.SimNumber,
                NoReg = entity.NoReg,
                SimType = entity.SimType,
                Height = entity.Height,
                ValidityPeriod = entity.ValidityPeriod,
                RowStatus = entity.RowStatus
            };
            DriversLicenseRepository.Add(list);
            UnitOfWork.SaveChanges();

            return UnitOfWork.SaveChanges() > 0;
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
