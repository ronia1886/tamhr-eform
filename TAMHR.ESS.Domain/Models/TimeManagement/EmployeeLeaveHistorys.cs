using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    [Table("TB_M_TIME_MANAGEMENT_EMPLOYEE_LEAVE_HISTORY")]
    public class EmployeeLeaveHistorys : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int Total { get; set; }
        public string LeaveTypeCode { get; set; }
        public DateTime CutOffDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
