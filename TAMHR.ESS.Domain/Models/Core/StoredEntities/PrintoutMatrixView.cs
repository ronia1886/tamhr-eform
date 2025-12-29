using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PRINT_OUT_MATRIX")]
    public partial class PrintoutMatrixView : IEntityMarker
    {
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
        public string FormTitle { get; set; }
        public string ApproverText { get; set; }
    }
}
