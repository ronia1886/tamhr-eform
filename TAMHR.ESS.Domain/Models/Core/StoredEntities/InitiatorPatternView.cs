using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_InitiatorPattern")]
    public class InitiatorPatternView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public Guid FormId { get; set; }
    }
}
