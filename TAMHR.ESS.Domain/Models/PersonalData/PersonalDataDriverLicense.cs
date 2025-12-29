using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_DRIVER_LICENSE")]
    public partial class PersonalDataDriverLicense : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string SimType { get; set; }
        public string SimNumber { get; set; }
        public string NoReg { get; set; }
        public string Height { get; set; }
        public DateTime? ValidityPeriod { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
