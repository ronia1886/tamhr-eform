using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ABSENT_SUMMARYAll", DatabaseObjectType.TableValued)]
    public class AbsenceSummaryAllStoredEntity
    {
        public Guid Id { get; set; }
        public int PresenceCode { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
        public string DisplayName { get { return string.Format("{0} ({1})", Name, PresenceCode); } }
    }
}
