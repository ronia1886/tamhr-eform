using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_FASKES_BPJS")]
    public partial class Faskes : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string FaskesCode { get; set; }
        public string FaskesCity { get; set; }
        public string FaskesName { get; set; }
        public string FaskesAddress { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
