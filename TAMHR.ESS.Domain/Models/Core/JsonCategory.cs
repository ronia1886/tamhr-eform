using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_JSON_CATEGORY")]
    public partial class JsonCategory : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Category { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Title { get; set; }

        [Column(TypeName = "varchar(450)")]
        public string Description { get; set; }
        
        [Column(TypeName = "varchar(MAX)")]
        public string JsonValues { get; set; }

        [Column(TypeName = "int")]
        public int OrderIndex { get; set; } = 1;

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }

        [Column(TypeName = "bit")]
        public bool RowStatus { get; set; }
    }
}
