using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PERSONAL_DETAIL_FAMILY_MEMBER")]
    public class PersonalDataFamilyDetailView : IEntityMarker
    {
        public Guid ID { get; set; }
        public string FamilyType { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string Nik { get; set; }
        public string Nama_Pegawai { get; set; }
        public string BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string ChildOrder { get; set; }
        public string LifeStatus { get; set; }
        public DateTime? DeathDate { get; set; }
        public string EducationLevel { get; set; }
        public string ChildStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Job { get; set; }
        public string BpjsNumber { get; set; }
        public string MemberNumber { get; set; }
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
