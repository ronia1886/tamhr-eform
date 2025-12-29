using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_TIME_MONITORING", DatabaseObjectType.StoredProcedure)]
    public class TimeManagementSummaryStoredEntity
    {
        public int Late { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
        public int EarlyLeave { get; set; }
        public int Abnormality { get; set; }
    }
}
