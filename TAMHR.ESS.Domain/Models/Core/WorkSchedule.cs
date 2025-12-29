using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_WORK_SCHEDULE")]
    public partial class WorkSchedule : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string MonthPeriod { get; set; }
        public string YearPeriod { get; set; }
        public string ShiftCode { get; set; }
        public string WorkScheduleRule { get; set; }
        public string PeriodWorkSchedule { get; set; }
        public string HolidayCalenderID { get; set; }
        public int? DayType { get; set; }
        public int? HolidayClass { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
