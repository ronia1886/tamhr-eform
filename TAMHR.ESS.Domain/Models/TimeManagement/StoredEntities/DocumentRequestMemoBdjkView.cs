using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_DOCUMENT_REQUEST_MEMO_BDJK")]
    public class DocumentRequestMemoBdjkView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string SubmitBy { get; set; }
        public string DocumentNumber { get; set; }
        public string ObjectValue { get; set; }
    }
}
