using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_HIT_COUNT_DETAIL")]
    public partial class HitCountDetailView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
