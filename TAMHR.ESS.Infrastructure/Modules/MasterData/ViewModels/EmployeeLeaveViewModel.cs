using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EmployeeLeaveViewModel
    {
        public Guid Id { get; set; }
        public string noreg { get; set; }
        public string leavetype { get; set; }
        public string Period { get; set; }
        public int TotalLeave { get; set; }
        public int UsedLeave { get; set; }
        public int RemainingLeave { get; set; }
        public string ModifiedBy { get; set; }
    }
}
