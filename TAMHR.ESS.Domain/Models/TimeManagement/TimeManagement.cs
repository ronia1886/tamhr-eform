using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TIME_MANAGEMENT")]
    public partial class TimeManagement : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string ShiftCode { get; set; }
        public string AbsentStatus { get; set; }
        public string Description { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public int? WorkHour { get; set; }
    }
}
