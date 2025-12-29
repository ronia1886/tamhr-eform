using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PERSONAL_DATA_DETAIL_INFORMATION")]
    public class PersonalDataInformationView : IEntityMarker
    {
        public Guid ID { get; set; }
        public string Nama_Pegawai { get; set; }
        public string Nama_Status { get; set; }
        public string Nk_SubKelas { get; set; }
        public string Tempat_Lahir { get; set; }
        public DateTime? Tanggal_Lahir { get; set; }
        public string Jenis_Kelamin { get; set; }
        public string Alamat { get; set; }
        public string Nama_Region { get; set; }
        public string Alamat_District { get; set; }
        public string Alamat_Subdistrict { get; set; }
        public string Alamat_AdministrativeVillage { get; set; }
        public string Kode_Pos { get; set; }
        public string Rt { get; set; }
        public string Rw { get; set; }
        public string AddressResidentialStatus { get; set; }
        public string Dom_Region { get; set; }
        public string Dom_District { get; set; }
        public string Dom_Subdistrict { get; set; }
        public string Dom_AdministrativeVillage { get; set; }
        public string Dom_PostalCode { get; set; }
        public string Dom_Rt { get; set; }
        public string Dom_Rw { get; set; }
        public string DomicileResidentialStatus { get; set; }
        public int TotalFamily { get; set; }
        public string Email { get; set; }
        public string Status_Nikah { get; set; }
        public string Agama { get; set; }
        public string Dana_Pensiun_Astra { get; set; }
        public string BPJS_Kesehatan { get; set; }
        public string BPJS_Ketenagakerjaan { get; set; }
        public string Lokasi { get; set; }
        public string No_Aviva { get; set; }
        public string Noreg { get; set; }
        public DateTime? Tanggal_Masuk_Astra { get; set; }
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
        public DateTime? StartDate { get; set; }
        public string Domisili { get; set; }
        public string PhoneNumber {  get; set; }
        public string IdentityCardName { get; set; }
        public string SimANumber { get; set; }
        public string SimCNumber { get; set; }
        public string PassportNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string WhatsappNumber { get; set; }
        public DateTime? TamDate { get; set; }
    }
}
