using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ANNUAL_WFH_PLANNING_DETAIL")]
    public partial class AnnualWFHPlanningDetail : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid AnnualWFHPlanningId { get; set; }
        public string NoReg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WorkPlace { get; set; }
        public int Days { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
