using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_ABSENT")]
    public partial class Absence : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int? CodePresensi { get; set; }
        public string Name { get; set; }
        public int? SubmitApplicationStart { get; set; }
        public int? SubmitApplicationEnd { get; set; }
        public int? MaxAbsentDays { get; set; }
        public bool? MandatoryAttachment { get; set; }
        public string AbsenceType { get; set; }
        public bool? Planning { get; set; }
        public bool? Unplanning { get; set; }
        public bool? ActiveValidation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
