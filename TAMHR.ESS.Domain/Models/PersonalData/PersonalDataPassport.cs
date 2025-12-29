using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_PASSPORT")]
    public partial class PersonalDataPassport : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string PassportNumber { get; set; }
        public string CountryCode { get; set; }
        public string Office { get; set; }
        public string NoReg { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
