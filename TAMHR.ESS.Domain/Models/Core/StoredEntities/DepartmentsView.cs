using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_DEPARTMENTS")]
    public partial class DepartmentsView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
    }
}
