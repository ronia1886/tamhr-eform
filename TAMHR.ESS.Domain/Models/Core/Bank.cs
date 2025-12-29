using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BANK")]
    public partial class Bank : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string BankKey { get; set; }
        public string BankName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Branch { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
      
    }
}
