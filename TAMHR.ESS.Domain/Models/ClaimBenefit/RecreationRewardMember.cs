using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_RECREATION_REWARD_MEMBER")]
    public partial class RecreationRewardMember : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid RecreationRewardId { get; set; }
        public string NoReg { get; set; }
        public string BenefitSubType { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
