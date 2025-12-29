using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EMPLOYEE_WORK_SCHEDULE")]
    public partial class EmployeeWorkScheduleView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string ShiftCode { get; set; }
        public bool Holiday { get; set; }
        public bool ChangeShift { get; set; }
        public string WorkScheduleRule { get; set; }
        public DateTime Date { get; set; }
        public bool Off { get { return (ShiftCode == "OFF" || ShiftCode == "OFFS") || (Holiday && !ChangeShift); } }
    }
}
