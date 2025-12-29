using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle form question master data.
    /// </summary>
    public class FormQuestionService : GenericDomainServiceBase<FormQuestion>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for form question entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Title",
            "QuestionTypeCode",
            "DefaultValue",
            "IsActive",
            "KeyCode"
        };

        protected string[] DetailProperties => new[] {
            "Description",
            "DescriptionType",
            "MinVal",
            "MaxVal",
            "MinTrue",
            "MaxTrue",
            "CriteriaType",
            "OrderSequence",
            "IsSubmittedDate",
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public FormQuestionService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of questions by form id and without parent.
        /// </summary>
        /// <param name="formId">This form id.</param>
        /// <returns>This list of <see cref="FormQuestion"/> objects.</returns>
        public IQueryable<FormQuestion> GetRoots(Guid formId)
        {
            return CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.FormId == formId && x.ParentFormQuestionId == null && x.RowStatus);
        }

        /// <summary>
        /// Get list of questions from parent id.
        /// </summary>
        /// <param name="parentFormQuestionId">This parent id.</param>
        /// <returns>This list of <see cref="FormQuestion"/> objects.</returns>
        public IQueryable<FormQuestion> GetQuestions(Guid parentFormQuestionId)
        {
            return CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ParentFormQuestionId == parentFormQuestionId && x.RowStatus);
        }

        public FormQuestionDetail GetQuestionDetailById(Guid id)
        {
            var set = UnitOfWork.GetRepository<FormQuestionDetail>();
            return set.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id && x.RowStatus).FirstOrDefault();
        }

        public IQueryable<FormQuestionDetail> GetQuestionDetails(Guid formQuestionId)
        {
            var set = UnitOfWork.GetRepository<FormQuestionDetail>();
            return set.Fetch()
                .AsNoTracking()
                .Where(x => x.FormQuestionId == formQuestionId && x.RowStatus);
        }

        public bool Reorder(string actor, Guid id, Guid? referenceId, int orderIndex)
        {
            return UnitOfWork.UspQuery("dbo.SP_REORDER_FORM_QUESTION", new { actor, id, referenceId, orderIndex });
        }

        public override void Upsert(FormQuestion data)
        {
            var isNew = data.Id == Guid.Empty;

            if (isNew)
            {
                data.CategoryCode = "default-category";

                if (data.ParentFormQuestionId.HasValue)
                {
                    var parentFormQuestion = CommonRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == data.ParentFormQuestionId);

                    data.FormId = parentFormQuestion.FormId;
                }
            }

            base.Upsert(data);

            if (isNew) {
                Reorder(data.CreatedBy, data.Id, data.ParentFormQuestionId, int.MaxValue);
            }
        }

        public IEnumerable<FormQuestionGroupStoredEntity> GetFormQuestionGroupAnswers(string groupAnswer, Guid formId)
        {
            return UnitOfWork.UdfQuery<FormQuestionGroupStoredEntity>(new { formId, groupAnswer });
        }

        public bool UpsertQuestionGroup(string groupAnswer, Guid[] ids)
        {
            var repo = UnitOfWork.GetRepository<FormQuestionGroupAnswer>();

            // Ambil id yang tidak ada di list, tapi tidak pakai Contains
            var toDelete = repo.Fetch()
                .Where(x => x.GroupAnswer == groupAnswer)
                .ToList()
                .Where(x => !ids.Any(i => i == x.FormQuestionId))
                .ToList();

            foreach (var row in toDelete)
            {
                repo.Delete(row);
            }

            // Ambil id yang sudah ada
            var existingIds = repo.Fetch()
                .AsNoTracking()
                .Where(x => x.GroupAnswer == groupAnswer)
                .Select(x => x.FormQuestionId)
                .Distinct()
                .ToList();

            // Tentukan id yang ingin dimasukkan tanpa pakai Contains di SQL
            var newIds = ids.Where(i => !existingIds.Any(e => e == i)).ToList();

            foreach (var id in newIds)
            {
                repo.Add(new FormQuestionGroupAnswer
                {
                    FormQuestionId = id,
                    GroupAnswer = groupAnswer,
                    Value = "true"
                });
            }

            return UnitOfWork.SaveChanges() > 0;
        }

        public bool UpsertDetail(FormQuestionDetail data)
        {
            // Push pending changes into database and return the boolean result.
            var set = UnitOfWork.GetRepository<FormQuestionDetail>();
            set.Upsert<Guid>(data, DetailProperties);

            return UnitOfWork.SaveChanges() > 0;
        }

        public bool DeleteDetailById(Guid Id)
        {
            // Push pending changes into database and return the boolean result.
            var set = UnitOfWork.GetRepository<FormQuestionDetail>();
            set.DeleteById(Id);

            return UnitOfWork.SaveChanges() > 0;
        }
        #endregion
    }
}
