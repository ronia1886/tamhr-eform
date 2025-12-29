using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ACTUAL_REPORTING_STRUCTURE")]
    public partial class ActualReportingStructureView : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string ParentNoReg { get; set; }
        public string ParentName { get; set; }
        public string ParentPostCode { get; set; }
        public string ParentPostName { get; set; }
        public string ParentJobCode { get; set; }
        public string ParentJobName { get; set; }
        public int HierarchyLevel { get; set; }
        public int PositionLevel { get; set; }
    }
}
