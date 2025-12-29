using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_VACCINE_SCHEDULE_LIMIT", DatabaseObjectType.TableValued)]
    public partial class VaccineScheduleLimitStoredEntity
    {
        public Guid Id { get; set; }
        public Guid VaccineScheduleId { get; set; }
        public Guid VaccineHospitalId { get; set; }
        public DateTime VaccineDate { get; set; }
        public int? Qty { get; set; }
        public int? RemainingQty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string HospitalName { get; set; }
    }
}
