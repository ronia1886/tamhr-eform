using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Common.Extensions;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Z.EntityFramework.Plus;
using TAMHR.ESS.Infrastructure.Helpers;
using Agit.Domain.Extensions;
using System.Text;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle form
    /// </summary>
    public class FormService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Form repository
        /// </summary>
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();

        /// <summary>
        /// Menu repository
        /// </summary>
        protected IRepository<Menu> MenuRepository => UnitOfWork.GetRepository<Menu>();
        #endregion

        #region Private Properties
        private readonly string[] _properties = new[] {
            "ModuleCode",
            "Title",
            "TitleFormat",
            "DocumentNumberFormat",
            "IsoNumber",
            "CanDownload",
            "IntegrationDownload",
            "StartDate",
            "EndDate",
            "StartTime",
            "EndTime"
        }; 
        #endregion

        #region Constructor
        public FormService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public bool AnyFormId(Guid id)
        {
            return FormRepository.Fetch()
                .Any(x => x.Id == id);
        }

        public bool AnyFormKey(Guid id, string formKey)
        {
            return FormRepository.Fetch()
                .Any(x => x.Id != id && x.FormKey == formKey);
        }

        /// <summary>
        /// Get all forms
        /// </summary>
        /// <returns>List of Forms</returns>
        public IEnumerable<Form> Gets()
        {
            var output = FormRepository.Fetch()
                .AsNoTracking()
                .GroupJoin(MenuRepository.Fetch().AsNoTracking(), x => "~/core/form/index?formKey=" + x.FormKey, y => y.Url, (form, menu) => new Form { Id = form.Id, FormKey = form.FormKey, Title = form.Title, TitleFormat = form.TitleFormat, DocumentNumberFormat = form.DocumentNumberFormat, IsoNumber = form.IsoNumber, ModuleCode = form.ModuleCode, CreatedOn = form.CreatedOn, RowStatus = form.RowStatus, IconClass = menu.FirstOrDefaultIfEmpty().IconClass });

            return output;
        }

        /// <summary>
        /// Get form by id
        /// </summary>
        /// <param name="id">Form id</param>
        /// <returns>Form Object</returns>
        public Form Get(Guid id)
        {
            return FormRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get form by key
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <returns>Form Object</returns>
        public Form Get(string formKey)
        {
            return FormRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.FormKey == formKey && x.RowStatus)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert form.
        /// </summary>
        /// <param name="actor">This actor.</param>
        /// <param name="form">This <see cref="Form"/> input object.</param>
        public void Upsert(string actor, Form form)
        {
            if (form.Id == default)
            {
                var formattedFormKey = form.FormKey.Ucwords();
                var formattedFormKeyWithSpace = form.FormKey.Ucwords(separator: " ");

                var parameters = new
                {
                    actor,
                    module = form.ModuleCode,
                    formKey = form.FormKey,
                    formattedFormKey,
                    formattedFormKeyWithSpace
                };

                UnitOfWork.UspQuery("dbo.SP_CREATE_PERMISSION_MENU", parameters);
                
                // Clear menu cache.
                QueryCacheManager.ExpireTag(new[] { "menus" });
            }

            FormRepository.Upsert<Guid>(form, _properties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete form by id and its dependencies if any
        /// </summary>
        /// <param name="id">Form Id</param>
        public void SoftDelete(Guid id)
        {
            var form = FormRepository.FindById(id);

            form.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete form by id and its dependencies if any
        /// </summary>
        /// <param name="id">Form Id</param>
        public void Delete(Guid id)
        {
            FormRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get ISO number by form key.
        /// </summary>
        /// <param name="formkey">This form key.</param>
        /// <returns>This ISO number.</returns>
        public string GetIsoNumber(string formkey)
        {
            var form = FormRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.FormKey == formkey)
                .FirstOrDefaultIfEmpty();

            return form.IsoNumber;
        }

        /// <summary>
        /// Get list of form questions by form key.
        /// </summary>
        /// <param name="formKey">This form key.</param>
        /// <returns>This list of <see cref="FormQuestion"/> objects.</returns>
        public IEnumerable<FormQuestion> GetFormQuestions(string formKey, bool IsActive=true)
        {
            var formQuestionSet = UnitOfWork.GetRepository<FormQuestion>();

            var formQuestions = formQuestionSet.Fetch()
                .AsNoTracking()
                .Include(x => x.Form)
                .Where(x => x.Form.FormKey == formKey && x.IsActive==IsActive)
                .OrderBy(x => x.OrderSequence);

            return formQuestions;
        }

        public IEnumerable<FormQuestion> GetFormQuestions(IEnumerable<Guid> ids)
        {
            var formQuestionSet = UnitOfWork.GetRepository<FormQuestion>();

            var idQuery = ids.AsQueryable();

            var formQuestions = formQuestionSet.Fetch()
                .AsNoTracking()
                .Include(x => x.ParentFormQuestion)
                //.Where(x => ids.Contains(x.Id))
                .Join(
                    idQuery,
                    x => x.Id,
                    id => id, 
                    (x, id) => x 
                )
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

        public bool UpsertFormLog(string actor, FormLog formLog)
        {
            var formLogSet = UnitOfWork.GetRepository<FormLog>();

            var hasOpenLog = formLogSet.Fetch()
                .Any(x => x.NoReg == formLog.NoReg && x.FormId == formLog.FormId && x.CreatedBy == actor && !x.Closed);

            Assert.ThrowIf(hasOpenLog && formLog.Id == default, "Cannot insert new log when previous log was not closed");

            formLogSet.Upsert<Guid>(formLog, new[] { "Notes", "Closed" });

            if (formLog.Closed)
            {
                var now = DateTime.Now;
                var currentDate = now.Date;
                var healthDeclarationSet = UnitOfWork.GetRepository<HealthDeclaration>();

                healthDeclarationSet.Fetch()
                    .Where(x => x.NoReg == formLog.NoReg && x.SubmissionDate >= x.CreatedOn.Date && x.SubmissionDate <= currentDate && x.RowStatus && !x.Marked)
                    .Update(x => new HealthDeclaration { Marked = true, ModifiedBy = actor, ModifiedOn = now });
            }

            return UnitOfWork.SaveChanges() > 0;
        }

        public FormLog GetOpenedFormLog(string actor, string noreg)
        {
            return GetFormLogs(actor, noreg).Where(x => !x.Closed)
                .FirstOrDefaultIfEmpty();
        }

        public IQueryable<FormLog> GetFormLogs(string actor)
        {
            var formLogSet = UnitOfWork.GetRepository<FormLog>();

            return formLogSet.Fetch()
                .AsNoTracking()
                .Where(x => x.CreatedBy == actor && x.RowStatus);
        }

        public IQueryable<FormLog> GetFormLogs(string actor, string noreg)
        {
            return GetFormLogs(actor).Where(x => x.NoReg == noreg);
        }

        public void ValidateCreateForm(string formKey)
        {
            var current = DateTime.Now;
            var now = current.Date.AddHours(current.Hour).AddMinutes(current.Minute);
            var form = FormRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.FormKey == formKey);

            var startDate = form.StartDate ?? now.AddDays(-1);
            var endDate = form.EndDate ?? now.AddDays(1);

            var startDateTime = form.StartTime != null ? now.Date.Add(form.StartTime.Value) : now.Date;
            var endDateTime = form.EndTime != null ? now.Date.Add(form.EndTime.Value) : now.Date.AddDays(1).AddSeconds(-1);
            //var validate = (startDate <= now.Date && endDate >= now.Date);
            //var LogService = new LogService(UnitOfWork);
            //var sb = new StringBuilder();
            //sb.Append(string.Format("{0:dd/MM/yyyy}-{1:dd/MM/yyyy}", startDate, endDate));
            //sb.Append(";");
            //sb.Append(startDate.ToString());
            //sb.Append(";");
            //sb.Append(endDate.ToString());
            //sb.Append(";");
            //sb.Append(validate.ToString());
            //LogService.LogSuccess("test", "test", "test", "test", sb.ToString());
            Assert.ThrowIf(!(startDate <= now.Date && endDate >= now.Date), string.Format("Cannot create form request, validity date period {0:dd/MM/yyyy}-{1:dd/MM/yyyy}", startDate, endDate));
            Assert.ThrowIf(!(now >= startDateTime && endDateTime >= now), string.Format("Cannot create form request, validity time from {0:HH:mm} until {1:HH:mm}", startDateTime, endDateTime));
        }

        public string ValidateLoadForm(string formKey)
        {
            var current = DateTime.Now;
            var now = current.Date.AddHours(current.Hour).AddMinutes(current.Minute);
            var form = FormRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.FormKey == formKey);

            var startDate = form.StartDate ?? now.AddDays(-1);
            var endDate = form.EndDate ?? now.AddDays(1);

            var startDateTime = form.StartTime != null ? now.Date.Add(form.StartTime.Value) : now.Date;
            var endDateTime = form.EndTime != null ? now.Date.Add(form.EndTime.Value) : now.Date.AddDays(1).AddSeconds(-1);
            var validate = (startDate <= now.Date && endDate >= now.Date);
            var LogService = new LogService(UnitOfWork);
            var sb = new StringBuilder();
            sb.Append(string.Format("{0:dd/MM/yyyy}-{1:dd/MM/yyyy}", startDate, endDate));
            sb.Append(";");
            sb.Append(startDate.ToString());
            sb.Append(";");
            sb.Append(endDate.ToString());
            sb.Append(";");
            sb.Append(validate.ToString());
            LogService.LogSuccess("test", "test", "test", "test", sb.ToString());
            if (!(startDate <= now.Date && endDate >= now.Date))
            {
                return string.Format("Cannot create form request, validity date period {0:dd/MM/yyyy}-{1:dd/MM/yyyy}", startDate, endDate);
            }

            if(!(now >= startDateTime && endDateTime >= now))
            { 
                  return   string.Format("Cannot create form request, validity time from {0:HH:mm} until {1:HH:mm}", startDateTime, endDateTime);
            }

            return "";
        }
        #endregion
    }
}
