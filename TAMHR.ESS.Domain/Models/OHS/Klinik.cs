using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_KLINIK")]
    public partial class KlinikView : IEntityMarker
    {
        //[Key]
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public string AreaId { get; set; }
        public string NamaArea { get; set; }
        public string Klinik { get; set; }
        public string FromHours { get; set; }
        public string ToHours { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public String PIC { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }

    [Table("TB_M_KLINIK")]
    public partial class KlinikTB : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AreaId { get; set; }
        public string Klinik { get; set; }
        public string CategoryCode { get; set; }
        public string FromHours { get; set; }
        public string ToHours { get; set; }
        public string PIC { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        [NotMapped]
        public Guid? AreaIdKlinik { get; set; }
    }
}
