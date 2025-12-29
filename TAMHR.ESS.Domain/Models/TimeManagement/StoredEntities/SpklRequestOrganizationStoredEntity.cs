using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_SPKL_BY_ORGANIZATION", DatabaseObjectType.TableValued)]
    public class SpklRequestOrganizationStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string EmployeeClass { get; set; }
        public DateTime OvertimeDate { get; set; }
        public DateTime OvertimeInAdjust { get; set; }
        public DateTime OvertimeOutAdjust { get; set; }
        public int OvertimeBreakAdjust { get; set; }
        public decimal DurationAdjust { get; set; }
        public string OvertimeCategory { get; set; }
        public string OvertimeReason { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
    }
}
