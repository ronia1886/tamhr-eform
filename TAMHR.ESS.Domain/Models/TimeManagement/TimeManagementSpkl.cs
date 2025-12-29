using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_SPKL")]
    public partial class TimeManagementSpkl : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime OvertimeDate { get; set; }
        public DateTime OvertimeInPlan { get; set; }
        public DateTime OvertimeOutPlan { get; set; }
        public DateTime OvertimeInAdjust { get; set; }
        public DateTime OvertimeOutAdjust { get; set; }
        public int OvertimeBreakPlan { get; set; }
        public int OvertimeBreakAdjust { get; set; }
        public decimal DurationPlan { get; set; }
        public decimal DurationAdjust { get; set; }
        public string OvertimeCategoryCode { get; set; }
        public string OvertimeReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static TimeManagementSpkl Create(SpklRequest spklRequest)
        {
            var timeManagementSpkl = new TimeManagementSpkl
            {
                NoReg = spklRequest.NoReg,
                OvertimeDate = spklRequest.OvertimeDate,
                OvertimeInPlan = spklRequest.OvertimeIn,
                OvertimeOutPlan = spklRequest.OvertimeOut,
                OvertimeInAdjust = spklRequest.OvertimeInAdjust.Value,
                OvertimeOutAdjust = spklRequest.OvertimeOutAdjust.Value,
                OvertimeBreakPlan = spklRequest.OvertimeBreak,
                OvertimeBreakAdjust = spklRequest.OvertimeBreakAdjust.Value,
                DurationPlan = spklRequest.Duration,
                DurationAdjust = spklRequest.DurationAdjust.Value,
                OvertimeCategoryCode = spklRequest.OvertimeCategoryCode,
                OvertimeReason = spklRequest.OvertimeReason
            };

            return timeManagementSpkl;
        }
    }
}
