using TAMHR.ESS.Domain;
using Agit.Domain.Event;

namespace TAMHR.ESS.Infrastructure.DomainEvents
{
    /// <summary>
    /// Document approval event object
    /// </summary>
    public class DocumentApprovalEvent : IDomainEvent
    {
        /// <summary>
        /// Document approval object
        /// </summary>
        public DocumentApproval DocumentApproval { get; }

        /// <summary>
        /// Actual organization object
        /// </summary>
        public ActualOrganizationStructure ActualOrganizationStructure { get; }

        /// <summary>
        /// Remarks if any
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Approval action code
        /// </summary>
        public string ActionCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actionCode">Approval Action Code</param>
        /// <param name="documentApproval">Document Approval</param>
        /// <param name="actualOrganizationStructure">Actual Organization Structure</param>
        /// <param name="remarks">Remarks</param>
        public DocumentApprovalEvent(string actionCode, DocumentApproval documentApproval, ActualOrganizationStructure actualOrganizationStructure, string remarks)
        {
            ActionCode = actionCode;
            DocumentApproval = documentApproval;
            ActualOrganizationStructure = actualOrganizationStructure;
            Remarks = remarks;
        }
    }
}
