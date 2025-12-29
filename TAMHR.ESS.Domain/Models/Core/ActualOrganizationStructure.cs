using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_ACTUAL_ORGANIZATION_STRUCTURE")]
    public partial class ActualOrganizationStructure : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string ParentOrgCode { get; set; }
        public string OrgName { get; set; }
        public string Service { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string EmployeeGroup { get; set; }
        public string EmployeeGroupText { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string WorkContract { get; set; }
        public string WorkContractText { get; set; }
        public string PersonalArea { get; set; }
        public string PersonalSubarea { get; set; }
        public int? DepthLevel { get; set; }
        public decimal Staffing { get; set; }
        public int Chief { get; set; }
        public int OrgLevel { get; set; }
        public int Vacant { get; set; }
        public string Structure { get; set; }
    }
}
