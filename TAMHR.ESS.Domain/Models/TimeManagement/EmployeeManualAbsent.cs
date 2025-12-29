using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    [Table("TB_M_TIME_MANAGEMENT_EMPLOYEE_MANUAL_ABSENT")]
    public class EmployeeManualAbsent : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int UsedEdited { get; set; }
        public string LeaveType { get; set; }
        public DateTime Period { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
