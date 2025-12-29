using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_ACTUAL_ENTITY_STRUCTURE")]
    public partial class ActualEntityStructure : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string ObjectCode { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
    }
}
