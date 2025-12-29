using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_SHIFT")]
    public partial class TimeManagementShift : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string ShiftName { get; set; }
        public string ShiftCode { get; set; }
        public DateTime ShiftDate { get; set; }
        public float ShitInProxy { get; set; }
        public float ShitOutProxy { get; set; }
        public string ShiftCodeActual { get; set; }
        public string StatusShift { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }

}
