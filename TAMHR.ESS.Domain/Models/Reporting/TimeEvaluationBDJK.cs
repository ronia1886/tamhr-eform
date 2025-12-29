using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TIME_EVALUATION_BDJK")]
    public partial class TimeEvaluationBDJK : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string NoReg { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public Boolean A { get; set; }
        public Boolean B { get; set; }
        public Boolean C { get; set; }
        public Boolean D { get; set; }
        public Boolean T { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
