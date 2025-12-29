using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{


    [Table("TB_M_HOSPITAL")]
    public partial class Hospital : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProviderType {get;set;}
        public string Province { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

    }
}
