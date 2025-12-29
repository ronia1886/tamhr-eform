using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_EMPLOYEE_WORK_SCHEDULE")]
    public partial class EmployeeWorkSchedule : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string PostName { get; set; }
        public string WorkScheduleRule { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static EmployeeWorkSchedule Create(EmployeeWorkSchedule input, string name, string postName)
        {
            input.Name = name;
            input.PostName = postName;

            return input;
        }
    }
}
