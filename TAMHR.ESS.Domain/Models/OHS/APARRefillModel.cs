using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("VW_APAR_REFILL")]
    public partial class APARRefillViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public int Qty { get; set; }
        public string Type { get; set; }
        public string QtySearch { get; set; }
        public string Merk { get; set; }
        public string Remark { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public string UseAparFor { get; set; }
        public bool RowStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
    }

    [Table("TB_R_APAR_REFILL")]
    public partial class APARRefillModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public int Qty { get; set; }
        public string Type { get; set; }
        public string Merk { get; set; }
        public string Remark { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; }
        public string UseAparFor { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
