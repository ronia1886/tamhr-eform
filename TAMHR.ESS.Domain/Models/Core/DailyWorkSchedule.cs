using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_DAILY_WORK_SCHEDULE")]
    public partial class DailyWorkSchedule : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public string ShiftDesc { get; set; }
        public int DurationNormal { get; set; }
        public TimeSpan? NormalTimeIN { get; set; }
        public TimeSpan? NormalTimeOut { get; set; }
        public string NormalTimeINText { get { return NormalTimeIN.HasValue ? NormalTimeIN.Value.ToString("hh\\:mm") : null; } }
        public string NormalTimeOutText { get { return NormalTimeOut.HasValue ? NormalTimeOut.Value.ToString("hh\\:mm") : null; } }
        public string ColorClass { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
