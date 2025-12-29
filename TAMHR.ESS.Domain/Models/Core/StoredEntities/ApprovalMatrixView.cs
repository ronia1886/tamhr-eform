using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_APPROVAL_MATRIX")]
    public partial class ApprovalMatrixView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string FormTitle { get; set; }
        public string InitiatorPattern { get; set; }
        public string InitiatorPatternName { get; set; }
        public string InitiatorType { get; set; }
        public string Approver { get; set; }
        public string ApproverText { get; set; }
        public string ApproverType { get; set; }
        public string ApproverTypeName { get; set; }
        public int ApproverLevel { get; set; }
        public bool ApproveAll { get; set; }
        public bool MandatoryInput { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
