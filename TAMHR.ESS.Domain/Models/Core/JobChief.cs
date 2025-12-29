using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_JOB_CHIEF")]
    public partial class JobChief : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string JobCode { get; set; }
        public bool Chief { get; set; }
        public string JobFamily { get; set; }
        public int JobLevel { get; set; }
    }
}
