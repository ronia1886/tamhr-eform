using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_FAMILY")]
    public class PersonalDataFamilyView : IEntityMarker
    {
        public Guid Id { get; set; }
		public string NoReg { get; set; }
		public Guid? CommonAttributeId { get; set; }
		public bool? IsMainFamily { get; set; }
		public string FamilyTypeCode { get; set; }
		public string FamilyType { get; set; }
		public string Name { get; set; }
		public string BirthPlace { get; set; }
		public DateTime BirthDate { get; set; }
		public string GenderCode { get; set; }
		public string Gender { get; set; }
		public DateTime? BirthRegistrationDate { get; set; }
		public string BirthRegistration { get; set; }
		public DateTime? CondolanceDate { get; set; }
		public string Condolance { get; set; }
		public DateTime? MarriageDate { get; set; }
		public string Marriage { get; set; }
		public DateTime? DivorceDate { get; set; }
		public string Divorce { get; set; }
		public string BpjsNumber { get; set; }
		public string InsuranceNumber { get; set; }
		public string NIK { get; set; }
		public string Address { get; set; }
		public string RT { get; set; }
		public string RW { get; set; }
		public string PostalCode { get; set; }
		public string KKNumber { get; set; }
		public string District { get; set; }
		public string Kota { get; set; }
		public string Kecamatan { get; set; }
        public string Provinsi { get; set; }
        public string Kelurahan { get; set; }
		public string ReligionCode { get; set; }
		public string BloodTypeCode { get; set; }
		public string PassportNumber { get; set; }
		public string PhoneNumber { get; set; }
	}
}
