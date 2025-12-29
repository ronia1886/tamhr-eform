using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_TRACKING_APPROVAL", DatabaseObjectType.StoredProcedure)]
    public class TrackingApprovalStoredEntity
    {
        public Guid ApprovalMatrixId { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public int ApprovalLevel { get; set; }
        public bool MandatoryInput { get; set; }
        public string ApprovalActionCode { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
