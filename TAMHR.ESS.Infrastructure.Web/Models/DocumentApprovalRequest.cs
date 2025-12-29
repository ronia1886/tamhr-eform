using System;

namespace TAMHR.ESS.Infrastructure.Web.Models
{
    public class DocumentApprovalRequest
    {
        public Guid DocumentApprovalId { get; set; }
        public string Remarks { get; set; }
        public Guid? RefId { get; set; }
        public string RefTable { get; set; }
    }
}
