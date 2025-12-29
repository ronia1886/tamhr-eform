using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_IDENTITY_INFORMATION")]
    public class PersonalDataIdentityView : IEntityMarker
    {
        public Guid Id { get; set; }
		public string NoReg { get; set; }
		public string Name { get; set; }
		public string KKHeadName { get; set; }
		public string Status_Nikah { get; set; }
		public string Jenis_Kelamin { get; set; }
		public string Agama { get; set; }
		public string Email { get; set; }
		public string Tempat_Lahir { get; set; }
		public DateTime? Tanggal_Lahir { get; set; }
		public int? Sisa_Cuti_Panjang { get; set; }
		public string No_KK { get; set; }
		public string No_KTP { get; set; }
		public string Alamat { get; set; }
		public string Rt { get; set; }
		public string Rw { get; set; }
		public string Kode_Pos { get; set; }
		public string Provinsi { get; set; }
		public string Kota { get; set; }
		public string Kecamatan { get; set; }
		public string Kelurahan { get; set; }
		public string PhoneNumber { get; set; }
		public string GolonganDarah { get; set; }
	}
}
