using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_HISTORY")]
    public partial class TimeManagementHistory : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string ShiftCode { get; set; }
        public string PresenceCode { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static TimeManagementHistory CreateFrom(TimeManagement timeManagement)
        {
            var timeManagementHistory = new TimeManagementHistory
            {
                NoReg = timeManagement.NoReg,
                WorkingDate = timeManagement.WorkingDate,
                WorkingTimeIn = timeManagement.WorkingTimeIn,
                WorkingTimeOut = timeManagement.WorkingTimeOut,
                PresenceCode = timeManagement.AbsentStatus,
                ShiftCode = timeManagement.ShiftCode,
                Description = timeManagement.Description
            };

            return timeManagementHistory;
        }
    }
}
