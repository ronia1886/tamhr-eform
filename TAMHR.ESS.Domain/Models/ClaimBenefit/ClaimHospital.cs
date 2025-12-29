using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_HOSPITAL")]
    public partial class ClaimHospital : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string FamilyMemberType { get; set; }
        public string PatientName { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
        public string ReimburshmentType { get; set; }
        public decimal Cost { get; set; }
        public decimal InsuranceAmmount { get; set; }
        public decimal Ammount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
