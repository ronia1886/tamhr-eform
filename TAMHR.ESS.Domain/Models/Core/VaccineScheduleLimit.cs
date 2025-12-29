using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_VACCINE_SCHEDULE_LIMIT")]
    public partial class VaccineScheduleLimit : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid VaccineScheduleId { get; set; }
        public Guid VaccineHospitalId { get; set; }
        public DateTime? VaccineDate { get; set; }
        public int? Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }      
    }
}
