using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMPLOYEE_HIERARCHIES", DatabaseObjectType.TableValued)]
    public class AnnualPlanningEmployeeHierarchiesStoredEntity
    {
        public Guid ID { get; set; }
        public string ActualNoReg { get; set; }
        public string ActualName { get; set; }
        public string ActualJobCode { get; set; }
        public string ActualJobName { get; set; }
        public string ActualPostCode { get; set; }
        public string ActualPostName { get; set; }
        public int? ActualStaffing { get; set; }
        public string TargetNoReg { get; set; }
        public string TargetName { get; set; }
        public string TargetJobCode { get; set; }
        public string TargetJobName { get; set; }
        public string TargetPostCode { get; set; }
        public string TargetPostName { get; set; }
        public int? TargetStaffing { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RelationType { get; set; }
        public int? np { get; set; }
    }
}
