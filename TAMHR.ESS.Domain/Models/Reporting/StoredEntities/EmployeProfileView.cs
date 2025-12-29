using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EMPLOYEE_STATUS_DETAIL1")]
    //[Table("VW_EMPLOYE_PERSONAL_DATA_STATUS")]
    public class EmployeProfileView : IEntityMarker
    {
        public Guid ID { get; set; }
        public string OrgCode { get; set; }
        public string ParentOrgCode { get; set; }
        public string OrgName { get; set; }
        public string Service { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string EmployeeGroup { get; set; }
        public string Expr1 { get; set; }
        public string EmployeeSubGroup { get; set; }
        public string EmployeeSubGroupText { get; set; }
        public string WorkContract { get; set; }
        public string WorkContractText { get; set; }
        public string PersonalArea { get; set; }
        public string PersonalSubArea { get; set; }
        public int DepthLevel { get; set; }
        public decimal Staffing { get; set; }
        public int Chief { get; set; }
        public DateTime Period { get; set; }
        public int Vacant { get; set; }
        public string Structure { get; set; }
        public string Divisi { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Directorate { get; set; }
        public string DirOrgCode { get; set; }
        public string DivOrgCode { get; set; }
        public string DepOrgCode { get; set; }
        public string SecOrgCode { get; set; }
        public int? NP { get; set; }
    }
}
