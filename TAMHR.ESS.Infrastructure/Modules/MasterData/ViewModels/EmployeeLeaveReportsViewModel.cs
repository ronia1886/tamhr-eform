using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EmployeeLeaveReportsViewModel
    {
        public string noreg { get; set; }
        public string Name { get; set; }
        public int TotalAnnualLeave { get; set; }
        public int UsedAnnualLeave { get; set; }
        public int AnnualLeave { get; set; }
        public int TotalLongLeave { get; set; }
        public int UsedLongLeave { get; set; }
        public int LongLeave { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? perioddateleave { get; set; }
    }
}
