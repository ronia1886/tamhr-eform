using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ANNUAL_WFH_PLANNING_DETAIL")]
    public class AnnualWFHPlanningDetailView : IEntityMarker
    {
        public Guid Id { get; set; }
        public int YearPeriod { get; set; }
        public string CreatedBy { get; set; }
        public string Superior { get; set; }
        public int Version { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public string WorkPlace { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public bool IsUpdated { get; set; }
        public string DocumentStatusCode { get; set; }
    }
}
