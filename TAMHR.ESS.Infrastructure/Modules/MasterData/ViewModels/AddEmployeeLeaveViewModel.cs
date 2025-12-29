using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AddEmployeeLeaveViewModel
    {
        public Guid Id { get; set; }
        public string noreg { get; set; }
        public string Period { get; set; }
        public int? TotalLeave { get; set; }
        public int UsedLeave { get; set; }
        public int RemainingLeave { get; set; }
        public string PeriodLongLeave { get; set; }
        public int? TotalLongLeave { get; set; }
        public int UsedLongLeave { get; set; }
        public int RemainingLongLeave { get; set; }
        public string ModifiedBy { get; set; }
    }
}
