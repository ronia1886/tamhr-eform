using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_DOCUMENT_APPROVAL_SAP")]
    public partial class DocumentApprovalSAPView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid FormId { get; set; }
        [Column(TypeName="varchar(50)")]
        public string DocumentNumber { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string Title { get; set; }
        public int Progress { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string SubmitBy { get; set; }
        public DateTime? SubmitOn { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string LastApprovedBy { get; set; }
        public DateTime? LastApprovedOn { get; set; }
        [Column(TypeName = "varchar(450)")]
        public string CurrentApprover { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string DocumentStatusCode { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public bool VisibleInHistory { get; set; }
        public bool EnableDocumentAction { get; set; }
        public bool CanSubmit { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string FormKey { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string FormTitle { get; set; }
        public bool CanDownload { get; set; }
        public int DownloadCount { get; set; }
        public bool IntegrationDownload { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string DocumentStatusTitle { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string CreatorName { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string SubmitterName { get; set; }
        public DateTime? TerminationDate { get; set; }
    }
}
