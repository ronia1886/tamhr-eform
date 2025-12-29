using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_AREA")]
    public partial class AreaView : IEntityMarker
    {
        //[Key]
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public string NamaArea { get; set; }
        public string DivisiCode { get; set; }
        public string DivisiName { get; set; }
        public string Alamat { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }

    [Table("TB_M_AREA")]
    public partial class AreaTB : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NamaArea { get; set; }
        public string DivisiCode { get; set; }
        public string Alamat { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }
}
