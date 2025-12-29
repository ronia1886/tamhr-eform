using Agit.Common;
using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_APPROVAL_MATRIX")]
    public partial class ApprovalMatrix : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string InitiatorPattern { get; set; }
        public string InitiatorType { get; set; }
        public string Approver { get; set; }
        public string ApproverType { get; set; }
        public int ApproverLevel { get; set; } = 1;
        public bool ApproveAll { get; set; }
        public bool MandatoryInput { get; set; }
        public string Excludes { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.ParseExact("12/31/9999", "MM/dd/yyyy", CultureInfo.CurrentCulture);
        public string Permissions { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
