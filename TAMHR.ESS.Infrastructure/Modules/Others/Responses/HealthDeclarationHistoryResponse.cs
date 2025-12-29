using System;

namespace TAMHR.ESS.Infrastructure.Responses
{
    public class HealthDeclarationHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid ReferenceDocumentApprovalId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime SubmissionDate { get; set; }
        public bool IsSick { get; set; }
        public bool HaveFever { get; set; }
        public string Remarks { get; set; }
    }
}
