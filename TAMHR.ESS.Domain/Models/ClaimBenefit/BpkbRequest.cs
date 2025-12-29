using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_BPKB_REQUEST")]
    public partial class BpkbRequest : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string RequestType { get; set; }
        public string NoReg { get; set; }
        public Guid BPKBId { get; set; }
        public string BPKBNo { get; set; }
        public string BorrowPurpose { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? ReturnDate{ get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [NotMapped]
        public string Name { get; set; }
    }
}
