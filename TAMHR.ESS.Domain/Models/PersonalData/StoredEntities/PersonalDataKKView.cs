using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_KK")]
    public class PersonalDataKKView : IEntityMarker
    {
        public Guid Id { get; set; }
		public string NoReg { get; set; }
		public string KKNumber { get; set; }
		public string KKStatusCode { get; set; }
		public string KKStatus { get; set; }
		public string Name { get; set; }
		public string Nik { get; set; }
		public string GenderCode { get; set; }
		public string Gender { get; set; }
		public DateTime? BirthDate { get; set; }
		public string BirthPlace { get; set; }
		public string ReligionCode { get; set; }
		public string Religion { get; set; }
		public string EducationCode { get; set; }
		public string Education { get; set; }
		public string WorkTypeCode { get; set; }
		public string Work { get; set; }
		public string BloodTypeCode { get; set; }
		public string MaritalStatusCode { get; set; }
		public string MaritalStatus { get; set; }
		public DateTime? MaritalDate { get; set; }
		public string NationalityCode { get; set; }
		public string Nationality { get; set; }
		public string PhoneNumber { get; set; }
		public bool RowStatus { get; set; }
		public string PassportNumber { get; set; }
		public string KitapNumber { get; set; }
		public string FatherName { get; set; }
		public string MatherName { get; set; }
        //public string Alamat { get; set; }
        //public string Rt { get; set; }
        //public string Rw { get; set; }
        //public string Kode_Pos { get; set; }
        //public string Provinsi { get; set; }
        //public string Kota { get; set; }
        //public string Kecamatan { get; set; }
        //public string Kelurahan { get; set; }
    }
}
