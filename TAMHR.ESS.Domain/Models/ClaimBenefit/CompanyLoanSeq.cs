using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_COMPANY_LOAN_SEQ")]
    public partial class CompanyLoanSeq : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public Guid? CompanyLoanId { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public int OrderSequence { get; set; }
        public string RequestStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
