using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using System.Collections.Generic;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_VACCINE_QUESTION")]
    public partial class VaccineQuestion : IEntityBase<Guid>
    {
        public VaccineQuestion()
        {
            VaccineQuestionDetailList = new List<VaccineQuestionDetail>();
        }

        [Key]
        public Guid Id { get; set; }
        public Guid VaccineId { get; set; }
        public Guid FormQuestionId { get; set; }
        public string Answer { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public List<VaccineQuestionDetail> VaccineQuestionDetailList { get; set; }
    }

    public partial class VaccineQuestion
    {
        public string Gender;
    }
}
