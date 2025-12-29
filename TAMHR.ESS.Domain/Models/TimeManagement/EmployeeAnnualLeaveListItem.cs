using System;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    public class EmployeeAnnualLeaveListItem
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; } = "";
        public string Name { get; set; } = "";
        public int Period { get; set; }
        public decimal? AnnualLeave { get; set; }
    }
}
