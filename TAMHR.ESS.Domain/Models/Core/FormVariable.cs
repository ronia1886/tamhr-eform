using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM_VARIABLE")]
    public partial class FormVariable : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string VariableName { get; set; }
        public string DataTypeCode { get; set; }
        public int? Length { get; set; }
        public string DataSource { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
