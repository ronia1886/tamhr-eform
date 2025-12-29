using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ABSENT_SUMMARY_DETAILS", DatabaseObjectType.TableValued)]
    public class AbsenceSummaryDetailsStoredEntity
    {
        public Guid Id { get; set; }
        public string PresenceCode { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
    }
}
