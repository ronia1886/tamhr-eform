using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
	[Table("VW_VACCINE_REPORT_EMPLOYEE")]
	public partial class VaccinReportEmployeeView : IEntityMarker
	{
		[Key]
		public Guid  VaccineId { get; set; }
		public string NoReg { get; set; }
		public long? OrderRank { get; set; }
		public string Status { get; set; }
		public int VaccineTo { get; set; }
		public string StatusCode { get; set; }
		public string VaccineType { get; set; }
		public string VaccineTypeCode { get; set; }
		public string EmployeeName { get; set; }
		public string Employee { get; set; }
		public string Name { get; set; }
		public string Class { get; set; }
		public string JobCode { get; set; }
		public string JobName { get; set; }
		public string Division { get; set; }
		public string Department { get; set; }
		public string Section { get; set; }
		public string Email { get; set; }
		public string FamilyStatus { get; set; }
		public DateTime? SubmissionDate { get; set; }
		public string Age { get; set; }
		public DateTime? BirthDate { get; set; }
		public string PhoneNumber { get; set; }
		public string IdentityId { get; set; }
		public string Domicile { get; set; }
		public string Address { get; set; }
		public string IdentityImage { get; set; }
		public bool? Eligible { get; set; }
		public string Allergies { get; set; }
		public DateTime? LastNegativeSwabDate { get; set; }
		public bool? IsPregnant { get; set; }
		public string OtherQuestion { get; set; }
		public bool? OtherVaccine { get; set; }
		public bool? VaccineAgreement { get; set; }
		public DateTime? VaccineDate1 { get; set; }
		public bool? IsSideEffects1 { get; set; }
		public string SideEffects1 { get; set; }
		public string VaccineHospital1 { get; set; }
		public string VaccineCard1 { get; set; }
		public string StatusUploadVaccineCard1 { get; set; }
		public DateTime? VaccineDate2 { get; set; }
		public bool? IsSideEffects2 { get; set; }
		public string SideEffects2 { get; set; }
		public string VaccineHospital2 { get; set; }
		public string VaccineCard2 { get; set; }
		public string StatusUploadVaccineCard2 { get; set; }
		public string Gender { get; set; }
		public string City { get; set; }
		public string District { get; set; }
		public string SubDistrict { get; set; }
		public string RiwayatPenyakit { get; set; }
		public string MonitoringNotes { get; set; }
		[NotMapped]
		public DateTime? VaccineDate1End { get; set; }
		[NotMapped]
		public DateTime? VaccineDate2End { get; set; }
		public string IsAutofill { get; set; }
		public string TAMVaccineAgreement { get; set; }

	}
}