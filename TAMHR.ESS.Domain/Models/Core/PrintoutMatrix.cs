using Agit.Common;
using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PRINT_OUT_MATRIX")]
    public partial class PrintoutMatrix : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string SubType { get; set; }
        public string ApproverLocation { get; set; }
        public string ApproverType { get; set; }
        public string Approver { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
