using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_HIT_COUNT_DETAIL")]
    public partial class HitCountDetail : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid HitCountId { get; set; }
        public string NoReg { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
