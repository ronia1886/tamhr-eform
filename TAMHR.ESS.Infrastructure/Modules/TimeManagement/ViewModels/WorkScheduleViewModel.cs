using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class WorkScheduleViewModel
    {
        public Guid  id { get; set; }
        public string NoReg { get; set; }
        public string WorkScheduleRule { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Date { get; set; }
        public string ShiftCode { get; set; }
        public string PeriodWorkSchedule { get; set; }
        public string ShiftName { get; set; }
        public string ShiftDesc { get; set; }
        public string NormalTimeIN { get; set; }
        public string NormalTimeOUT { get; set; }
        public string DurationNormal { get; set; }
        public string Holiday { get; set; }
        public string ChangeShift { get; set; }
    }
}
