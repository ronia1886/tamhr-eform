using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_COMPANY_LOAN_DETAIL")]
    public partial class CompanyLoanDetail : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyLoadId { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string PostCode { get; set; }
        public string Address { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
