using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_MONITORING_SUBORDINATE_MONTHLY", DatabaseObjectType.TableValued)]
    public class TimeMonitoringSubordinateMonthlyStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string WorkContract { get; set; }
        public int JobLevel { get; set; }
        public int TotalIn { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalLate { get; set; }
    }
}
