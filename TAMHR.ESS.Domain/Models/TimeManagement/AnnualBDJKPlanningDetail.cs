using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ANNUAL_BDJK_PLANNING_DETAIL")]
    public partial class AnnualBDJKPlanningDetail : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid AnnualBDJKPlanningId { get; set; }
        public string NoReg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BDJKCode { get; set; }
        public string Activity { get; set; }
        public int Days { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
