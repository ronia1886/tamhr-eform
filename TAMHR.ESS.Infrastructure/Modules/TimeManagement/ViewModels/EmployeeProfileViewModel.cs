using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EmployeeProfileViewModel
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
        public bool? IsEligible { get; set; }
        public int? Absent3 { get; set; }
        public int? Absent5 { get; set; }
        public int? Absent11 { get; set; }
        public int? Absent28 { get; set; }
        public int? Absent34 { get; set; }
        public int? Absent35 { get; set; }
        public int? Absent36 { get; set; }
        public int? Absent37 { get; set; }
        public DateTime? StartDateParam { get; set; }
        public DateTime? EndDateParam { get; set; }
    }
}
