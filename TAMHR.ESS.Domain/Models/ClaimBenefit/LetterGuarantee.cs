using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_LETTER_GUARANTEE")]
    public partial class LetterGuarantee : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string FamilyMemberType { get; set; }
        public string PatientName { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalCity { get; set; }
        public string BenefitClassification { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
        public string ControlCriteria { get; set; }
        public string SurgeryType { get; set; }
        public int ControlNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
