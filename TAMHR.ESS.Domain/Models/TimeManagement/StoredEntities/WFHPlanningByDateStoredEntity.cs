
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_ANNUAL_WFH_PLANNING_BY_DATE", DatabaseObjectType.StoredProcedure)]
    public class WFHPlanningByDate
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
