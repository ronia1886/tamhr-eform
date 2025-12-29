using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_CONFIG")]
    public partial class Config : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string ModuleCode { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string ConfigKey { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string ConfigText { get; set; }
        [Column(TypeName = "varchar(4000)")]
        public string ConfigValue { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string DataTypeCode { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
