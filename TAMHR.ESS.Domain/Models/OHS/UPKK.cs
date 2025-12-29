using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("TB_R_UPKK")]
    public partial class UPKK : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        // Menggunakan DateTime, hanya menyimpan tanggal
        //public DateTime TanggalKunjungan
        //{
        //    get => TanggalKunjungan.Date;  // Mengembalikan hanya bagian tanggal
        //    set => TanggalKunjungan = value.Date;  // Mengatur hanya bagian tanggal
        //}
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public string NamaEmployeeVendor { get; set; }
        public DateTime? TanggalLahirVendor { get; set; }
        public int ? Usia { get; set; }
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

    [Table("VW_VISIT_UPKK")]
    public partial class VisitUpkkView : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string LokasiUPKK { get; set; }
        public string KategoriKunjungan { get; set; }
        // Menggunakan DateTime, hanya menyimpan tanggal
        //public DateTime TanggalKunjungan
        //{
        //    get => TanggalKunjungan.Date;  // Mengembalikan hanya bagian tanggal
        //    set => TanggalKunjungan = value.Date;  // Mengatur hanya bagian tanggal
        //}
        public DateTime? TanggalKunjungan { get; set; }
        public string Company { get; set; }
        public string Divisi { get; set; }
        public string Noreg { get; set; }
        public int Usia { get; set; }
        public string JenisPekerjaan { get; set; }
        public string AreaId { get; set; }
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
        public bool RowStatus { get; set; }


    }
}
