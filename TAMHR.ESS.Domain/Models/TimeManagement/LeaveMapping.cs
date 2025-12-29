using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_LEAVE_MAPPING")]
    public partial class LeaveMapping : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid ActualLeaveId { get; set; }
        public Guid AnnualLeavePlanningDetailId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
