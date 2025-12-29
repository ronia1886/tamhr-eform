using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_APPROVAL")]
    public partial class DocumentApproval : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid FormId { get; set; }
        [Column(TypeName = "varchar(50)")]
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
        public bool VisibleInHistory { get; set; } = true;
        public bool EnableDocumentAction { get; set; } = true;
        public bool CanSubmit { get; set; } = true;
        public string FormKey { get { return Form?.FormKey; } }
        public bool CanDownload { get { return Form != null ? Form.CanDownload : true; } }
        [JsonIgnore]
        public DocumentRequestDetail ObjDocumentRequestDetail { get; set; }
        [JsonIgnore]
        public Form Form { get; set; }
        [JsonIgnore]
        public List<DocumentApprovalHistory> DocumentApprovalHistories { get; set; }
        [JsonIgnore]
        public IEnumerable<DocumentApprovalFile> DocumentApprovalFiles { get; set; }
    }
}
