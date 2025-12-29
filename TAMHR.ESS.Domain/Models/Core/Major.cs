using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("RMS_TB_M_MAJOR")]
    public partial class Major : IEntityBase<Guid>
    {
        [Key]
        [Column(nameof(Id), TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(nameof(Code), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Code { get; set; }

        [Column(nameof(Category), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Category { get; set; }

        [Column(nameof(Name), TypeName = "varchar(150)")]
        [MaxLength(150)]
        public string Name { get; set; }

        [Column(nameof(CreatedBy), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string CreatedBy { get; set; }

        [Column(nameof(CreatedOn), TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }

        [Column(nameof(ModifiedBy), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column(nameof(ModifiedOn), TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }

        [Column(nameof(RowStatus), TypeName = "bit")]
        public bool RowStatus { get; set; }

    }
}
