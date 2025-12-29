using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_EMPLOYEE_LEAVE")]
    public partial class EmployeeLeave : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int AnnualLeave { get; set; }
        public int LongLeave { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [NotMapped]
        public string Name { get; set; }
        public static EmployeeLeave Create(EmployeeLeave employeeLeave, string name)
        {
            employeeLeave.Name = name;

            return employeeLeave;
        }
    }
}
