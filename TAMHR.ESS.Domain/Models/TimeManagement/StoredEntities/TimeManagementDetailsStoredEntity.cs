using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_MANAGEMENT_DETAILS", DatabaseObjectType.TableValued)]
    public class TimeManagementDetailsStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Class { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string WorkingTimeInLocation { get; set; }
        public string WorkingTimeOutLocation { get; set; }
        public int? WorkingHour { get; set; }
        public string ShiftCode { get; set; }
        public string PresenceCode { get; set; }
        public string Description { get; set; }
    }
}
