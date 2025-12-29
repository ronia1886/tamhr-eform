using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ACCESS_ROLE")]
    public partial class AccessRoleView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string AccessCode { get; set; }
        public string AccessTypeCode { get; set; }
        public Guid RoleId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string AccessName { get; set; }
        public string RoleKey { get; set; }
        public string RoleTitle { get; set; }
    }
}
