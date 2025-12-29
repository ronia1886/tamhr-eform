using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TERMINATION_TYPE")]
    public partial class TerminationType : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string TerminationTypeKey { get; set; }
        public string TerminationTypeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
