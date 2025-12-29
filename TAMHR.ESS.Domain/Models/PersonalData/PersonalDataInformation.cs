using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{

    [Table("VW_PERSONAL_DATA_INFORMATION_DETAILS")]
    //[Table("VW_EMPLOYE_PERSONAL_DATA_STATUS")]
    public class PersonalDataInformation : IEntityMarker
    {
        public Guid ID { get; set; }
        public string Nama_Pegawai { get; set; }
        public string Nama_Status { get; set; }
        public string Nk_SubKelas { get; set; }
        public string Tempat_Lahir { get; set; }
        public DateTime Tanggal_Lahir { get; set; }
        public string Jenis_Kelamin { get; set; }
        public string Alamat { get; set; }
        public string Email { get; set; }
        public string Status_Nikah { get; set; }
        public string Agama { get; set; }
        public string Dana_Pensiun_Astra { get; set; }
        public string BPJS_Kesehatan { get; set; }
        public string BPJS_Ketenagakerjaan { get; set; }
        public string Lokasi { get; set; }
        public string No_Aviva { get; set; }
        public string Noreg { get; set; }
        public DateTime Tanggal_Masuk_Astra { get; set; }
        public string No_KK { get; set; }
        public string BloodTypeCode { get; set; }
        public string NationalityCode { get; set; }
        public string Nik { get; set; }
        public string Npwp { get; set; }
        public string AccountNumber { get; set; }
        public string Branch { get; set; }
        public string TaxStatus { get; set; }
        public string PersArea { get; set; }
        public string Expr1 { get; set; }
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
        public string Nama_Pendidikan { get; set; }
        public string Universitas { get; set; }
        public string Jurusan { get; set; }

    }
}
