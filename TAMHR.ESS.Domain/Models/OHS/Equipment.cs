using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EQUIPMENT")]
    public partial class EquipmentView : IEntityMarker
    {
        //[Key]
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public string AreaId { get; set; }
        public string NamaArea { get; set; }
        public string DivisiCode { get; set; }
        public string DivisiName { get; set; }
        public string EquipmentName { get; set; }
        public int Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }

    [Table("TB_M_EQUIPMENT")]
    public partial class Equipment : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AreaId { get; set; }
        public string DivisiCode { get; set; }
        public string EquipmentName { get; set; }
        public int Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        [NotMapped]
        public Guid? AreaIdEquip { get; set; }
        [NotMapped]
        public string DivisiCodeEquip { get; set; }


    }
}
