using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("TB_R_PATIENT")]
    public partial class Patient : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Provider { get; set; }
        // Menggunakan DateTime, hanya menyimpan tanggal
        //public DateTime AdmissionDate
        //{
        //    get => AdmissionDate.Date;  // Mengembalikan hanya bagian tanggal
        //    set => AdmissionDate = value.Date;  // Mengatur hanya bagian tanggal
        //}
        public DateTime? AdmissionDate { get; set; }
        //public DateTime DisChargeAbleDate
        //{
        //    get => DisChargeAbleDate.Date;  // Mengembalikan hanya bagian tanggal
        //    set => DisChargeAbleDate = value.Date;  // Mengatur hanya bagian tanggal
        //}
        public DateTime? DisChargeAbleDate { get; set; }
        public string DiagnosisDesc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        [NotMapped]
        public string NoregPatient { get; set; }

    }
}
