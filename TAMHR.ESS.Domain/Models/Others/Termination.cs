using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TERMINATION")]
    public partial class Termination : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime EndDate { get; set; }
        public Guid TerminationTypeId { get; set; }
        public string Reason { get; set; }
        public Guid AttachmentCommonFileId { get; set; }
        public Guid VerklaringCommonFileId { get; set; }
        public Guid InterviewCommonFileId { get; set; }
        public string Position { get; set; }
        public string Division { get; set; }
        public string Class { get; set; }
        public string Email { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
