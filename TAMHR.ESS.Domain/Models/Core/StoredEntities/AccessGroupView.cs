using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ACCESS_GROUP")]
    public partial class AccessGroupView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
