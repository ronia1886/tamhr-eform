using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_APPROVAL_PARTICIPANT")]
    public partial class DocumentApprovalParticipant : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
