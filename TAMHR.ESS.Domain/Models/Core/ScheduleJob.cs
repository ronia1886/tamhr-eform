using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_SCHEDULE_JOB")]
    public partial class ScheduleJob : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string JobName { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
