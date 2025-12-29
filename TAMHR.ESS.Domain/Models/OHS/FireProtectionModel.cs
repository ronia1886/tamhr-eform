using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("VW_FIRE_PROTECTION")]
    public partial class FireProtectionViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public int Installed { get; set; }
        public int Ready { get; set; }
        public string InstalledSearch { get; set; }
        public string ReadySearch { get; set; }
        public string Category { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public bool RowStatus { get; set; }
        public string Readiness { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
    }

    [Table("TB_R_FIRE_PROTECTION")]
    public partial class FireProtectionModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public int Installed { get; set; }
        public int Ready { get; set; }
        public string Category { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; }
        public string CreatedBy { get; set; }
        public bool RowStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
