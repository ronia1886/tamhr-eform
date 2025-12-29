using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EMPLOYEE_LEAVE_HISTORY")]
    public class EmployeeLeaveHistoryView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string noreg { get; set; }
        public string Nama_Pegawai { get; set; }
        public string leavetype { get; set; }
        public int Period { get; set; }
        public int TotalLeave { get; set; }
        public int UsedLeave { get; set; }
        public int RemainingLeave { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? PeriodDateLeave { get; set; }
    }
}
