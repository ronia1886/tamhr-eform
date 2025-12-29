using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TRACKING_APPROVAL")]
    public partial class TrackingApproval : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public int ApprovalLevel { get; set; }
        public string ApprovalActionCode { get; set; }
        public string Remarks { get; set; }
        public Guid? ApprovalMatrixId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static TrackingApproval Create(Guid documentApprovalId, int approvalLevel, ActualOrganizationStructure actualOrganizationStructure, string approvalActionCode = null, string remarks = null)
        {
            var trackingApproval = new TrackingApproval
            {
                DocumentApprovalId = documentApprovalId,
                ApprovalLevel = approvalLevel,
                NoReg = actualOrganizationStructure.NoReg,
                Name = actualOrganizationStructure.Name,
                PostCode = actualOrganizationStructure.PostCode,
                PostName = actualOrganizationStructure.PostName,
                JobCode = actualOrganizationStructure.JobCode,
                JobName = actualOrganizationStructure.JobName,
                ApprovalActionCode = approvalActionCode,
                Remarks = remarks
            };

            return trackingApproval;
        }
    }
}
