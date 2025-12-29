using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_KATEGORI_PENYAKIT")]
    public partial class Kategori_PenyakitVIEW : IEntityMarker
    {
        //[Key]
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public Guid IdTingkatSakit { get; set; }
        public string Name { get; set; }
        public string KategoriPenyakit { get; set; }
        public string Penyakit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }

    [Table("TB_M_KATEGORI_PENYAKIT")]
    public partial class Kategori_Penyakit : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid IdTingkatSakit { get; set; }
        public string KategoriPenyakit { get; set; }
        public string Penyakit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }

    [Table("VW_KP_GROUP")]
    public partial class KpGroupView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
