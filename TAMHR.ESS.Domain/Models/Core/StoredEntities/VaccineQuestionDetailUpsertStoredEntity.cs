using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_VACCINE_QUESTION_DETAIL_UPSERT", DatabaseObjectType.StoredProcedure)]
    public partial class VaccineQuestionDetailUpsertStoredEntity
    {
        public Guid Id { get; set; }
        public Guid VaccineQuestionId { get; set; }
        public Guid FormQuestionDetailId { get; set; }
        public string Answer { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
