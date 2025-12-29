using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM")]
    public partial class Form : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModuleCode { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string FormKey { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Title { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string TitleFormat { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string DocumentNumberFormat { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string IsoNumber { get; set; }

        public bool CanDownload { get; set; }

        public bool IntegrationDownload { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool RowStatus { get; set; }

        public bool NeedApproval { get; set; }
        [NotMapped]
        public string IconClass { get; set; }
    }
}
