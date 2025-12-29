using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ANNUAL_LEAVE_PLANNING_DETAIL")]
    public partial class AnnualLeavePlanningDetail : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid AnnualLeavePlanningId { get; set; }
        public Guid AbsentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
