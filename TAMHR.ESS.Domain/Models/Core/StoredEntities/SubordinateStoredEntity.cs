using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_SUBORDINATES", DatabaseObjectType.TableValued)]
    public class SubordinateStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string OrgName { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Extension { get; set; }
        public DateTime EntryDate { get; set; }
    }

    [DatabaseObject("SF_GET_SUBORDINATES_FAMILIES", DatabaseObjectType.TableValued)]
    public class SubordinateFamilyStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string OrgName { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Extension { get; set; }
        public DateTime EntryDate { get; set; }
        public string TaxStatus { get; set; }
        public string FamilyName { get; set; }
        public string FamilyTypeCode { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
