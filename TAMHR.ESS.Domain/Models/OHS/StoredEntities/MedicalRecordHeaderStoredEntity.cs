using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{

    [DatabaseObject("SP_MEDICAL_RECORD_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordSummaryStoredEntity
    {
        public string YearMonth { get; set; }
        public int MCU { get; set; }
        public int UPKK { get; set; }
        public int SickLeave { get; set; }
        public int InOutPatient { get; set; }
        public int TanyaOHS { get; set; }


    }
    [DatabaseObject("SP_UPKK_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordUpkkSummaryStoredEntity
    {
        public string YearMonth { get; set; }
        public int PemeriksaanKesehatan { get; set; }
        public int BerobatTertangani { get; set; }
        public int BerobatRujuk { get; set; }
        public int BerobatPulang { get; set; }

    }
    [DatabaseObject("SP_SICKLEAVE_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordSickLeaveSummaryStoredEntity
    {
        public string YearMonth { get; set; }
        public int SakitBerat { get; set; }
        public int SakitRingan { get; set; }

    }
    [DatabaseObject("SP_SICKNESS_RATE_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordSickNessRateSummaryStoredEntity
    {
        public string YearMonth { get; set; }
        public float SickNessRate { get; set; }
       
    }

    [DatabaseObject("SP_MEDICAL_RECORD_TOTAL_PATIENT_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordTotalPatientSummaryStoredEntity
    {
       
        public int Total { get; set; }
    
    }
    [DatabaseObject("SP_MEDICAL_RECORD_TOTAL_SICKlEAVE_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordTotalSickLeaveSummaryStoredEntity
    {

        public int Total { get; set; }

    }

    [DatabaseObject("SP_MEDICAL_RECORD_TOTAL_SICKNESS_RATE_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordTotalSickNessRateSummaryStoredEntity
    {

        public float Total { get; set; }

    }

    [DatabaseObject("SP_MEDICAL_RECORD_TOTAL_UPKK_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordTotalUpkkSummaryStoredEntity
    {

        public int Total { get; set; }

    }

    [DatabaseObject("SP_MEDICAL_RECORD_HEADER", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordHeaderStoredEntity
    {
        public string NoReg { get; set; }
        public string Division { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public int ClassNp { get; set; }
        public DateTime? StartDateParam { get; set; }
        public DateTime? EndDateParam { get; set; }

    }


    [DatabaseObject("SP_MCU_DETAIL", DatabaseObjectType.StoredProcedure)]
    public class McuDetailStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string YearPeriod { get; set; }
        public int? GlukosaPuasa { get; set; }
        public int? SGOT { get; set; }
        public int? SGPT { get; set; }
        public string HBsAg { get; set; }
        public string AntiHBs { get; set; }
        public decimal? AsamUrat { get; set; }
        public int? KolesterolTotal { get; set; }
        public decimal? StatusBMI { get; set; }
        public string TekananDarah { get; set; }
        public decimal? Kreatinin { get; set; }
        public string HasilEKG { get; set; }
        public string HasilRontgen { get; set; }
        public string HealthEmployeeStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string Paket { get; set; }
        public string LokasiMCU { get; set; }
        public int? Usia { get; set; }
        public string PenyakitPernahDiderita { get; set; }
        public string Perokok { get; set; }
        public int JumlahBatangPerHari { get; set; }
        public string Miras { get; set; }
        public string Olahraga { get; set; }
        public int FrequencyPerMinggu { get; set; }
        public string Kebisingan { get; set; }
        public string SuhuExtremePanasAtauDingin { get; set; }
        public string Radiasi { get; set; }
        public string GetaranLokal { get; set; }
        public string GetaranSeluruhTubuh { get; set; }
        public string LainnyaFisika { get; set; }
        public string Debu { get; set; }
        public string Asap { get; set; }
        public string LimbahB3 { get; set; }
        public string LainnyaKimia { get; set; }
        public string BakteriAtaVirusAtauJamurAtauParasit { get; set; }
        public string LainnyaBiologi { get; set; }
        public string GerakanBerulangDenganTangan { get; set; }
        public string AngkatBerat { get; set; }
        public string DudukLama { get; set; }
        public string BerdiriLama { get; set; }
        public string PosisiTubuhTidakErgonomis { get; set; }
        public string PencahayaanTidakSesuai { get; set; }
        public string BekerjaDepanLayar { get; set; }
        public string LainnyaErgonomis { get; set; }
        public string RiwayatHipertensi { get; set; }
        public string RiwayatDiabetes { get; set; }
        public string RiwayatPenyakitJantung { get; set; }
        public string RiwayatPenyakitGinjal { get; set; }
        public string RiwayatGangguanMental { get; set; }
        public string RiwayatPenyakitLain { get; set; }
        public string PenyakitSaatIni { get; set; }
        public string SedangBerobat { get; set; }
        public string ObatYangDiberikan { get; set; }
        public decimal? Tinggi { get; set; }
        public decimal? Berat { get; set; }
        public int TekananDarahSistol { get; set; }
        public int TekananDarahDiastol { get; set; }
        public string KesimpulanEKG { get; set; }
        public string KesimpulanTreadmill { get; set; }
        public string KesanPhotoRontgen { get; set; }
        public string KesimpulanUsgAbdomen { get; set; }
        public string HasilUsgMammae { get; set; }
        public string KesimpulanUsgMammae { get; set; }
        public string KesimpulanFisik { get; set; }
        public string KesimpulanButaWarna { get; set; }
        public string KesimpulanPemVisusMata { get; set; }
        public string HasilPapsmear { get; set; }
        public string KesimpulanPamsmear { get; set; }
        public decimal Hemoglobine { get; set; }
        public int Hematocrit { get; set; }
        public int Leucocyte { get; set; }
        public decimal TotalPlatelets { get; set; }
        public decimal Eryrocyte { get; set; }
        public decimal MCV { get; set; }
        public int MCH { get; set; }
        public int MCHC { get; set; }
        public int ESR { get; set; }
        public decimal? HbA1c { get; set; }
        public int? GammaGT { get; set; }
        public int? Ureum { get; set; }
        public decimal? GFR { get; set; }
        public int? HDL { get; set; }
        public int? LDL { get; set; }
        public int? Triglyceride { get; set; }
        public string PlateletAggregation { get; set; }
        public int Fibrinogen { get; set; }
        public decimal? CEA { get; set; }
        public string PSA { get; set; }
        public int? Ca125 { get; set; }
        public decimal? VitD25OH { get; set; }
        public string UrinDarah { get; set; }
        public string UrinBakteri { get; set; }
        public string UrinKristal { get; set; }
        public string UrinLeukosit { get; set; }
        public int ScoreAmbiguity { get; set; }
        public string KesimpulanAmbiguity { get; set; }
        public int ScoreConflict { get; set; }
        public string KesimpulanConflict { get; set; }
        public int ScoreQuantitative { get; set; }
        public string KesimpulanQuantitative { get; set; }
        public int ScoreQualitative { get; set; }
        public string KesimpulanQualitative { get; set; }
        public int ScoreCareerDevelopment { get; set; }
        public string KesimpulanCareerDevelopment { get; set; }
        public int ScoreResponsibilityforPeople { get; set; }
        public string KesimpulanResponsibilityforPeople { get; set; }
        public string KesimpulanLab { get; set; }
        public string Saran { get; set; }

    }

    [DatabaseObject("SP_UPKK_DETAIL", DatabaseObjectType.StoredProcedure)]
    public class UpkkDetailStoredEntity
    {
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        public DateTime TanggalKunjungan { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public int Usia { get; set; }
        public string JenisPekerjaan { get; set; }
        public string LokasiKerja { get; set; }
        public string Keluhan { get; set; }
        public string TDSistole { get; set; }
        public string TDDiastole { get; set; }
        public string Nadi { get; set; }
        public string Respirasi { get; set; }
        public string Suhu { get; set; }
        public string Diagnosa { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
        public string JenisKasus { get; set; }
        public string Treatment { get; set; }
        public string Pemeriksa { get; set; }
        public string NamaPemeriksa { get; set; }
        public string HasilAkhir { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }
    [DatabaseObject("SP_PATIENT_DETAIL", DatabaseObjectType.StoredProcedure)]
    public class PatientDetailStoredEntity
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Provider { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime DisChargeAbleDate { get; set; }
        public string DiagnosisDesc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }

    [DatabaseObject("SP_SICKLEAVE_DETAIL", DatabaseObjectType.StoredProcedure)]
    public class SickLeaveDetailStoredEntity
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Type { get; set; }
        public string KategoriPenyakit { get; set; }
        public string Penyakit { get; set; }
        public string TotalAbsen { get; set; }
        public string Diagnosa { get; set; }



    }


    [DatabaseObject("SP_TANYA_OHS_DETAIL", DatabaseObjectType.StoredProcedure)]
    public class TanyaOhsDetailStoredEntity
    {
        public string Nama { get; set; }
        public string Noreg { get; set; }
        public string Keluhan { get; set; }
        public string KategoriLayanan { get; set; }
        public string Status { get; set; }
        public string Rating { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }



    }


    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_MCU", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadMCUStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Divisi { get; set; }
        public string YearPeriod { get; set; }
        public int? GlukosaPuasa { get; set; }
        public int? SGOT { get; set; }
        public int? SGPT { get; set; }
        public string HBsAg { get; set; }
        public string AntiHBs { get; set; }
        public decimal? AsamUrat { get; set; }
        public int? KolesterolTotal { get; set; }
        public decimal? StatusBMI { get; set; }
        public string TekananDarah { get; set; }
        public decimal? Kreatinin { get; set; }
        public string HasilEKG { get; set; }
        public string HasilRontgen { get; set; }
        public string HealthEmployeeStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string Paket { get; set; }
        public string LokasiMCU { get; set; }
        public int? Usia { get; set; }
        public string PenyakitPernahDiderita { get; set; }
        public string Perokok { get; set; }
        public int? JumlahBatangPerHari { get; set; }
        public string Miras { get; set; }
        public string Olahraga { get; set; }
        public int? FrequencyPerMinggu { get; set; }
        public string Kebisingan { get; set; }
        public string SuhuExtremePanasAtauDingin { get; set; }
        public string Radiasi { get; set; }
        public string GetaranLokal { get; set; }
        public string GetaranSeluruhTubuh { get; set; }
        public string LainnyaFisika { get; set; }
        public string Debu { get; set; }
        public string Asap { get; set; }
        public string LimbahB3 { get; set; }
        public string LainnyaKimia { get; set; }
        public string BakteriAtaVirusAtauJamurAtauParasit { get; set; }
        public string LainnyaBiologi { get; set; }
        public string GerakanBerulangDenganTangan { get; set; }
        public string AngkatBerat { get; set; }
        public string DudukLama { get; set; }
        public string BerdiriLama { get; set; }
        public string PosisiTubuhTidakErgonomis { get; set; }
        public string PencahayaanTidakSesuai { get; set; }
        public string BekerjaDepanLayar { get; set; }
        public string LainnyaErgonomis { get; set; }
        public string RiwayatHipertensi { get; set; }
        public string RiwayatDiabetes { get; set; }
        public string RiwayatPenyakitJantung { get; set; }
        public string RiwayatPenyakitGinjal { get; set; }
        public string RiwayatGangguanMental { get; set; }
        public string RiwayatPenyakitLain { get; set; }
        public string PenyakitSaatIni { get; set; }
        public string SedangBerobat { get; set; }
        public string ObatYangDiberikan { get; set; }
        public decimal? Tinggi { get; set; }
        public decimal? Berat { get; set; }
        public int? TekananDarahSistol { get; set; }
        public int? TekananDarahDiastol { get; set; }
        public string KesimpulanEKG { get; set; }
        public string KesimpulanTreadmill { get; set; }
        public string KesanPhotoRontgen { get; set; }
        public string KesimpulanUsgAbdomen { get; set; }
        public string HasilUsgMammae { get; set; }
        public string KesimpulanUsgMammae { get; set; }
        public string KesimpulanFisik { get; set; }
        public string KesimpulanButaWarna { get; set; }
        public string KesimpulanPemVisusMata { get; set; }
        public string HasilPapsmear { get; set; }
        public string KesimpulanPamsmear { get; set; }
        public decimal? Hemoglobine { get; set; }
        public int? Hematocrit { get; set; }
        public int? Leucocyte { get; set; }
        public decimal? TotalPlatelets { get; set; }
        public decimal? Eryrocyte { get; set; }
        public decimal? MCV { get; set; }
        public int? MCH { get; set; }
        public int? MCHC { get; set; }
        public int? ESR { get; set; }
        public decimal? HbA1c { get; set; }
        public int? GammaGT { get; set; }
        public int? Ureum { get; set; }
        public decimal? GFR { get; set; }
        public int? HDL { get; set; }
        public int? LDL { get; set; }
        public int? Triglyceride { get; set; }
        public string PlateletAggregation { get; set; }
        public int? Fibrinogen { get; set; }
        public decimal? CEA { get; set; }
        public string PSA { get; set; }
        public int? Ca125 { get; set; }
        public decimal? VitD25OH { get; set; }
        public string UrinDarah { get; set; }
        public string UrinBakteri { get; set; }
        public string UrinKristal { get; set; }
        public string UrinLeukosit { get; set; }
        public int? ScoreAmbiguity { get; set; }
        public string KesimpulanAmbiguity { get; set; }
        public int? ScoreConflict { get; set; }
        public string KesimpulanConflict { get; set; }
        public int? ScoreQuantitative { get; set; }
        public string KesimpulanQuantitative { get; set; }
        public int? ScoreQualitative { get; set; }
        public string KesimpulanQualitative { get; set; }
        public int? ScoreCareerDevelopment { get; set; }
        public string KesimpulanCareerDevelopment { get; set; }
        public int? ScoreResponsibilityforPeople { get; set; }
        public string KesimpulanResponsibilityforPeople { get; set; }
        public string KesimpulanLab { get; set; }
        public string Saran { get; set; }
        public DateTime? Tanggal_Lahir { get; set; }
        public string Jenis_Kelamin { get; set; }



    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_SICK_LEAVE", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadSickLeaveStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Divisi { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AbsentDuration { get; set; }
        public string ReasonCode { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_UPKK_TAB_ALL", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadUpkkAllStoredEntity
    {
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public string NamaEmployeeVendor { get; set; }
        public DateTime? TanggalLahirVendor { get; set; }
        public int? Usia { get; set; }
        public string JenisKelaminEmployeeVendor { get; set; }
        public string JenisPekerjaan { get; set; }
        public Guid? AreaId { get; set; }
        public string LokasiKerja { get; set; }
        public string Keluhan { get; set; }
        public string TDSistole { get; set; }
        public string TDDiastole { get; set; }
        public string Nadi { get; set; }
        public string Respirasi { get; set; }
        public string Suhu { get; set; }
        public string Diagnosa { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
        public string JenisKasus { get; set; }
        public string Treatment { get; set; }
        public string Pemeriksa { get; set; }
        public string NamaPemeriksa { get; set; }
        public string HasilAkhir { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }



        [NotMapped]
        public string NoregUpkk { get; set; }
        [NotMapped]
        public string DivisiUpkk { get; set; }
        [NotMapped]
        public string Kategori { get; set; }
        [NotMapped]
        public string LokasiUpkkGroup { get; set; }
        [NotMapped]
        public int TAM { get; set; }
        [NotMapped]
        public int Outsource { get; set; }
        [NotMapped]
        public int Total { get; set; }

        [NotMapped] public int TAM_TSD { get; set; }
        [NotMapped] public int Outsource_TSD { get; set; }
        [NotMapped] public int TAM_PPDD { get; set; }
        [NotMapped] public int Outsource_PPDD { get; set; }
        [NotMapped] public int TAM_HRGA { get; set; }
        [NotMapped] public int Outsource_HRGA { get; set; }
        [NotMapped] public int TAM_LNTC { get; set; }
        [NotMapped] public int Outsource_LNTC { get; set; }
        [NotMapped] public int TAM_MENTENG { get; set; }
        [NotMapped] public int Outsource_MENTENG { get; set; }
        [NotMapped] public int TAM_SPLD { get; set; }
        [NotMapped] public int Outsource_SPLD { get; set; }
        [NotMapped] public int TAM_TTC { get; set; }
        [NotMapped] public int Outsource_TTC { get; set; }
        [NotMapped] public int TAM_SUNTER3 { get; set; }
        [NotMapped] public int Outsource_SUNTER3 { get; set; }
        [NotMapped] public int TAM_CCY { get; set; }
        [NotMapped] public int Outsource_CCY { get; set; }
        [NotMapped] public int TAM_KARAWANG { get; set; }
        [NotMapped] public int Outsource_KARAWANG { get; set; }
        [NotMapped] public int TAM_NGORO { get; set; }
        [NotMapped] public int Outsource_NGORO { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_UPKK_TAB1", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadUpkkTab1StoredEntity
    {
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public string NamaEmployeeVendor { get; set; }
        public DateTime? TanggalLahirVendor { get; set; }
        public int? Usia { get; set; }
        public string JenisKelaminEmployeeVendor { get; set; }
        public string JenisPekerjaan { get; set; }
        public Guid? AreaId { get; set; }
        public string LokasiKerja { get; set; }
        public string Keluhan { get; set; }
        public string TDSistole { get; set; }
        public string TDDiastole { get; set; }
        public string Nadi { get; set; }
        public string Respirasi { get; set; }
        public string Suhu { get; set; }
        public string Diagnosa { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
        public string JenisKasus { get; set; }
        public string Treatment { get; set; }
        public string Pemeriksa { get; set; }
        public string NamaPemeriksa { get; set; }
        public string HasilAkhir { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }



        [NotMapped]
        public string NoregUpkk { get; set; }
        [NotMapped]
        public string DivisiUpkk { get; set; }
        [NotMapped]
        public string Kategori { get; set; }
        [NotMapped]
        public string LokasiUpkkGroup { get; set; }
        [NotMapped]
        public int TAM { get; set; }
        [NotMapped]
        public int Outsource { get; set; }
        [NotMapped]
        public int Total { get; set; }

        [NotMapped] public int TAM_TSD { get; set; }
        [NotMapped] public int Outsource_TSD { get; set; }
        [NotMapped] public int TAM_PPDD { get; set; }
        [NotMapped] public int Outsource_PPDD { get; set; }
        [NotMapped] public int TAM_HRGA { get; set; }
        [NotMapped] public int Outsource_HRGA { get; set; }
        [NotMapped] public int TAM_LNTC { get; set; }
        [NotMapped] public int Outsource_LNTC { get; set; }
        [NotMapped] public int TAM_MENTENG { get; set; }
        [NotMapped] public int Outsource_MENTENG { get; set; }
        [NotMapped] public int TAM_SPLD { get; set; }
        [NotMapped] public int Outsource_SPLD { get; set; }
        [NotMapped] public int TAM_TTC { get; set; }
        [NotMapped] public int Outsource_TTC { get; set; }
        [NotMapped] public int TAM_SUNTER3 { get; set; }
        [NotMapped] public int Outsource_SUNTER3 { get; set; }
        [NotMapped] public int TAM_CCY { get; set; }
        [NotMapped] public int Outsource_CCY { get; set; }
        [NotMapped] public int TAM_KARAWANG { get; set; }
        [NotMapped] public int Outsource_KARAWANG { get; set; }
        [NotMapped] public int TAM_NGORO { get; set; }
        [NotMapped] public int Outsource_NGORO { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_UPKK_TAB2", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadUpkkTab2StoredEntity
    {
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public string NamaEmployeeVendor { get; set; }
        public DateTime? TanggalLahirVendor { get; set; }
        public int? Usia { get; set; }
        public string JenisKelaminEmployeeVendor { get; set; }
        public string JenisPekerjaan { get; set; }
        public Guid? AreaId { get; set; }
        public string LokasiKerja { get; set; }
        public string Keluhan { get; set; }
        public string TDSistole { get; set; }
        public string TDDiastole { get; set; }
        public string Nadi { get; set; }
        public string Respirasi { get; set; }
        public string Suhu { get; set; }
        public string Diagnosa { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
        public string JenisKasus { get; set; }
        public string Treatment { get; set; }
        public string Pemeriksa { get; set; }
        public string NamaPemeriksa { get; set; }
        public string HasilAkhir { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }



        [NotMapped]
        public string NoregUpkk { get; set; }
        [NotMapped]
        public string DivisiUpkk { get; set; }
        [NotMapped]
        public string Kategori { get; set; }
        [NotMapped]
        public string LokasiUpkkGroup { get; set; }
        [NotMapped]
        public int TAM { get; set; }
        [NotMapped]
        public int Outsource { get; set; }
        [NotMapped]
        public int Total { get; set; }

        [NotMapped] public int TAM_TSD { get; set; }
        [NotMapped] public int Outsource_TSD { get; set; }
        [NotMapped] public int TAM_PPDD { get; set; }
        [NotMapped] public int Outsource_PPDD { get; set; }
        [NotMapped] public int TAM_HRGA { get; set; }
        [NotMapped] public int Outsource_HRGA { get; set; }
        [NotMapped] public int TAM_LNTC { get; set; }
        [NotMapped] public int Outsource_LNTC { get; set; }
        [NotMapped] public int TAM_MENTENG { get; set; }
        [NotMapped] public int Outsource_MENTENG { get; set; }
        [NotMapped] public int TAM_SPLD { get; set; }
        [NotMapped] public int Outsource_SPLD { get; set; }
        [NotMapped] public int TAM_TTC { get; set; }
        [NotMapped] public int Outsource_TTC { get; set; }
        [NotMapped] public int TAM_SUNTER3 { get; set; }
        [NotMapped] public int Outsource_SUNTER3 { get; set; }
        [NotMapped] public int TAM_CCY { get; set; }
        [NotMapped] public int Outsource_CCY { get; set; }
        [NotMapped] public int TAM_KARAWANG { get; set; }
        [NotMapped] public int Outsource_KARAWANG { get; set; }
        [NotMapped] public int TAM_NGORO { get; set; }
        [NotMapped] public int Outsource_NGORO { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_UPKK_TAB3", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadUpkkTab3StoredEntity
    {
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public string NamaEmployeeVendor { get; set; }
        public DateTime? TanggalLahirVendor { get; set; }
        public int? Usia { get; set; }
        public string JenisKelaminEmployeeVendor { get; set; }
        public string JenisPekerjaan { get; set; }
        public Guid? AreaId { get; set; }
        public string LokasiKerja { get; set; }
        public string Keluhan { get; set; }
        public string TDSistole { get; set; }
        public string TDDiastole { get; set; }
        public string Nadi { get; set; }
        public string Respirasi { get; set; }
        public string Suhu { get; set; }
        public string Diagnosa { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
        public string JenisKasus { get; set; }
        public string Treatment { get; set; }
        public string Pemeriksa { get; set; }
        public string NamaPemeriksa { get; set; }
        public string HasilAkhir { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }



        [NotMapped]
        public string NoregUpkk { get; set; }
        [NotMapped]
        public string DivisiUpkk { get; set; }
        [NotMapped]
        public string Kategori { get; set; }
        [NotMapped]
        public string LokasiUpkkGroup { get; set; }
        [NotMapped]
        public int TAM { get; set; }
        [NotMapped]
        public int Outsource { get; set; }
        [NotMapped]
        public int Total { get; set; }

        [NotMapped] public int TAM_TSD { get; set; }
        [NotMapped] public int Outsource_TSD { get; set; }
        [NotMapped] public int TAM_PPDD { get; set; }
        [NotMapped] public int Outsource_PPDD { get; set; }
        [NotMapped] public int TAM_HRGA { get; set; }
        [NotMapped] public int Outsource_HRGA { get; set; }
        [NotMapped] public int TAM_LNTC { get; set; }
        [NotMapped] public int Outsource_LNTC { get; set; }
        [NotMapped] public int TAM_MENTENG { get; set; }
        [NotMapped] public int Outsource_MENTENG { get; set; }
        [NotMapped] public int TAM_SPLD { get; set; }
        [NotMapped] public int Outsource_SPLD { get; set; }
        [NotMapped] public int TAM_TTC { get; set; }
        [NotMapped] public int Outsource_TTC { get; set; }
        [NotMapped] public int TAM_SUNTER3 { get; set; }
        [NotMapped] public int Outsource_SUNTER3 { get; set; }
        [NotMapped] public int TAM_CCY { get; set; }
        [NotMapped] public int Outsource_CCY { get; set; }
        [NotMapped] public int TAM_KARAWANG { get; set; }
        [NotMapped] public int Outsource_KARAWANG { get; set; }
        [NotMapped] public int TAM_NGORO { get; set; }
        [NotMapped] public int Outsource_NGORO { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_INOUT_PATIENT", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadInOutPatientStoredEntity
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string Jenis_Kelamin { get; set; }
        public DateTime? Tanggal_Lahir { get; set; }
        public string Provider { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public DateTime? DisChargeAbleDate { get; set; }
        public string DiagnosisDesc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }

    [DatabaseObject("SP_MEDICAL_RECORD_DOWNLOAD_TANYA_OHS", DatabaseObjectType.StoredProcedure)]
    public class MedicalRecordDownloadTanyaOhsStoredEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string DoctorId { get; set; }
        public string Name { get; set; }
        public string NoReg { get; set; }
        public string Keluhan { get; set; }
        public string Solve { get; set; }
        public string Feedback { get; set; }
        public string Rating { get; set; }
        public string ReplyFeedback { get; set; }
        public string KategoriLayanan { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }

}

