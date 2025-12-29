using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_USER_ACTIVITY_LOG")]
    public partial class UserActivityLog : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string LogTypeCode { get; set; }
        public string Description { get; set; }
        public string JsonParams { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
