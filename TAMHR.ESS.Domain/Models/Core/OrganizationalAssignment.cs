using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_ORGANIZATIONAL_ASSIGNMENT")]
    public partial class OrganizationalAssignment : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PersonnelArea { get; set; }
        public string EmployeeGroup { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string PersonnelSubarea { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string JobCode { get; set; }
        public string EmployeeName { get; set; }
        public string LabourType { get; set; }
    }
}
