using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{ 

[Table("VW_PERSONAL_DATA_DETAIL_EDUCATION")]
public class PersonalDataEducationDetailView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string Education { get; set; }
        public string Institute { get; set; }
        public string Major { get; set; }
        public string Country { get; set; }
        public string WorkContractText { get; set; }
        public string DirOrgCode { get; set; }
        public string DivOrgCode { get; set; }
        public string DepOrgCode { get; set; }
        public string SecOrgCode { get; set; }
        public string OrgCode { get; set; }
        public string ParentOrgCode { get; set; }
        public string OrgName { get; set; }
        public string Divisi { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Directorate { get; set; }
        public string Nk_SubKelas { get; set; }
        public string Expr1 { get; set; }
    }
}
