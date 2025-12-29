using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_PERFORMANCE_DEVELOPMENT")]
    public partial class PerformanceDevelopment : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int Period { get; set; }
        public string AreaDevelopment { get; set; }
        public string GeneralDevCategory { get; set; }
        public string DevelopmentActivity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Mentor { get; set; }
        public string Target { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
