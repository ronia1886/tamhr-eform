using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_COP_FUEL")]
    public partial class CopFuel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string PostName { get; set; }
        public DateTime COPFuelDate { get; set; }
        public string Purpose { get; set; }
        public string DestinationStart { get; set; }
        public double KMStart { get; set; }
        public string DestinationEnd { get; set; }
        public double KMEnd { get; set; }
        public double KMTotal { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
