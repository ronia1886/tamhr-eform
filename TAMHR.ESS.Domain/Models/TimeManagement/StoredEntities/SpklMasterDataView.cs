using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_SPKL_MASTER_DATA")]
    public class SpklMasterDataView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public DateTime OvertimeDate { get; set; }
        public DateTime OvertimeInPlan { get; set; }
        public DateTime OvertimeOutPlan { get; set; }
        public DateTime OvertimeInAdjust { get; set; }
        public DateTime OvertimeOutAdjust { get; set; }
        public string ShiftCode { get; set; }
        public int OvertimeBreakPlan { get; set; }
        public int OvertimeBreakAdjust { get; set; }
        public decimal DurationPlan { get; set; }
        public decimal DurationAdjust { get; set; }
        public string OvertimeCategory { get; set; }
        public string OvertimeCategoryCode { get; set; }
        public string OvertimeReason { get; set; }
    }
}
