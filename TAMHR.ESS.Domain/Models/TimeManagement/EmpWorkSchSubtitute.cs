using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TIME_MANAGEMENT_EMP_WORK_SCHEDULE_SUBSTITUTE")]
    public partial class EmpWorkSchSubtitute : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime Date { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftCodeUpdate { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static EmpWorkSchSubtitute CreateFrom(string createdBy, DateTime createdOn, string oldShiftCode, TimeManagement timeManagement)
        {
            var output = new EmpWorkSchSubtitute
            {
                NoReg = timeManagement.NoReg,
                Date = timeManagement.WorkingDate,
                ShiftCode = oldShiftCode,
                ShiftCodeUpdate = timeManagement.ShiftCode,
                CreatedBy = createdBy,
                CreatedOn = createdOn
            };

            return output;
        }
    }
}
