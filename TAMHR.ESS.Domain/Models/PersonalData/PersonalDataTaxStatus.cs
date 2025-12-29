using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_TAX_STATUS")]
    public partial class PersonalDataTaxStatus : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Npwp { get; set; }
        public string TaxStatus { get; set; }
        public string TaxPtkp { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
