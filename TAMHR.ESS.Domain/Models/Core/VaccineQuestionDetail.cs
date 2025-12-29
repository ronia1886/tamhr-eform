using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
namespace TAMHR.ESS.Domain
{
    [Table("TB_R_VACCINE_QUESTION_DETAIL")]
    public partial class VaccineQuestionDetail : IEntityBase<Guid>
    {
        [Key]
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
