using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_MONITORING_SUBORDINATE", DatabaseObjectType.TableValued)]
    public class TimeMonitoringSubordinateStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Group { get; set; }
        public string Line { get; set; }
        public DateTime? ProxyIn { get; set; }
        public DateTime? ProxyOut { get; set; }
        public int JobLevel { get; set; }
        public int TotalProxy { get; set; }
    }
}
