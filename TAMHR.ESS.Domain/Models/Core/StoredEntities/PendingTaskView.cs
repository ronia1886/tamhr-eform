using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PENDING_TASK")]
    public partial class PendingTaskView : IEntityMarker
    {
        [Key]
        public string NoReg { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TotalPending { get; set; }
    }
}
