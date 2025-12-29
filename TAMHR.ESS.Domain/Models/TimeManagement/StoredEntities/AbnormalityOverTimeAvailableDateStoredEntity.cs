using Agit.Common.Attributes;
using System;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_AVAILABLE_DATE_ABNORMALITY_OVERTIME", DatabaseObjectType.TableValued)]
    public class AbnormalityOverTimeAvailableDateStoredEntity
    {
        public DateTime WorkingDate { get; set; }
        public DateTime NormalTimeIn { get; set; }
        public DateTime NormalTimeOut { get; set; }
        public DateTime WorkingTimeIn { get; set; }
        public DateTime WorkingTimeOut { get; set; }
    }
}
