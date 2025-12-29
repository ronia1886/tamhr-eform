using Agit.Common.Attributes;
using System;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EVENTS_CALENDAR", DatabaseObjectType.TableValued)]
    public partial class EventsCalendarStoredEntity
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string NoReg { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public string ShiftCode { get; set; }
    }
}
