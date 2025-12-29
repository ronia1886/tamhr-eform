using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_GENERAL_CATEGORY")]
    public partial class GeneralCategory : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string Code { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Category { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string Description { get; set; }
        public bool Readonly { get; set; }
        public int OrderSequence { get; set; } = 1;
        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
