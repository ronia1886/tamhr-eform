using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_KK_MEMBER")]
    public partial class PersonalDataKkMember : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
		public string KKNumber { get; set; }
		public string KKStatusCode { get; set; }
		public string Name { get; set; }
		public string Nik { get; set; }
		public string GenderCode { get; set; }
		public DateTime? BirthDate { get; set; }
		public string BirthPlace { get; set; }
		public string ReligionCode { get; set; }
		public string EducationCode { get; set; }
		public string WorkTypeCode { get; set; }
		public string BloodTypeCode { get; set; }
		public string MaritalStatusCode { get; set; }
		public DateTime? MaritalDate { get; set; }
		public string NationalityCode { get; set; }
		public string PhoneNumber { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public bool RowStatus { get; set; }
		public string PassportNumber { get; set; }
		public string KitapNumber { get; set; }
		public string FatherName { get; set; }
		public string MatherName { get; set; }
	}
}
