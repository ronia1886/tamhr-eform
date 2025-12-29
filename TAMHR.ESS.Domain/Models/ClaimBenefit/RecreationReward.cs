using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_RECREATION_REWARD")]
    public partial class RecreationReward : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string PostName { get; set; }
        public string BenefitType { get; set; }
        public string Bank { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public Guid DocumentApprovalId { get; set; }
    }
}
