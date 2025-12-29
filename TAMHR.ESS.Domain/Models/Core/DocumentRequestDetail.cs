using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_REQUEST_DETAIL")]
    public partial class DocumentRequestDetail : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceTable { get; set; }
        public string RequestTypeCode { get; set; }
        public string ObjectValue { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [JsonIgnore]
        public virtual DocumentApproval DocumentApproval { get; set; }
    }
}
