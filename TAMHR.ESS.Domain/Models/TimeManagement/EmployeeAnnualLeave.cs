using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_EMPLOYEE_ANNUAL_LEAVE")]
    public partial class EmployeeAnnualLeave : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int AnnualLeave { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public int Period { get; set; }

        [NotMapped]
        public string Name { get; set; }
        public static EmployeeAnnualLeave Create(EmployeeAnnualLeave employeeAnnualLeave, string name)
        {
            employeeAnnualLeave.Name = name;

            return employeeAnnualLeave;
        }
    }
}
