using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_VACCINE_SCHEDULE_DETAIL_GRID", DatabaseObjectType.TableValued)]
    public partial class VaccineScheduleDetailGridStoredEntity
    {
        public Guid VaccineScheduleId { get; set; }
        public string HospitalName { get; set; }
        public int? Qty { get; set; }
        public int? Total { get; set; }
        public int? RemainingQty { get; set; }
       
    }
}
