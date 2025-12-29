using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMPLOYEE_BY_ORG", DatabaseObjectType.TableValued)]
    public class EmployeeOrganizationStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public int Staffing { get; set; }
        public int JobLevel { get; set; }
        public bool Chief { get; set; }
        public int PositionLevel { get; set; }
    }

    [DatabaseObject("SF_GET_EMPLOYEE_BY_SUBGROUP", DatabaseObjectType.TableValued)]
    public class EmployeeOrganizationLevelStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public int Staffing { get; set; }
        public int JobLevel { get; set; }
        public bool Chief { get; set; }
        public int PositionLevel { get; set; }
    }
}
