using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMPLOYEE_LEAVES", DatabaseObjectType.TableValued)]
    public partial class EmployeeLeaveStoredEntity
    {
        public string Noreg { get; set; }
        public DateTime WorkingDate { get; set; }
        public string ReasonCode { get; set; }
        public string AbsentStatus { get; set; }
        public string Description { get; set; }
        public string DocumentStatus { get; set; }
    }
}