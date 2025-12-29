using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_APPROVAL_HISTORY")]
    public partial class DocumentApprovalHistory : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string ApprovalActionCode { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static DocumentApprovalHistory Create(Guid documentApprovalId, string approvalActionCode, ActualOrganizationStructure actualOrganizationStructure, string remarks)
        {
            var documentApprovalHistory = new DocumentApprovalHistory
            {
                DocumentApprovalId = documentApprovalId,
                ApprovalActionCode = approvalActionCode,
                NoReg = actualOrganizationStructure.NoReg,
                Name = actualOrganizationStructure.Name,
                PostCode = actualOrganizationStructure.PostCode,
                PostName = actualOrganizationStructure.PostName,
                JobCode = actualOrganizationStructure.JobCode,
                JobName = actualOrganizationStructure.JobName,
                Remarks = remarks
            };

            return documentApprovalHistory;
        }
    }
}
