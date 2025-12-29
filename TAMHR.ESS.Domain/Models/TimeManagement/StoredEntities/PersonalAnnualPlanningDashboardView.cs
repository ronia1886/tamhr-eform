using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PERSONAL_ANNUAL_PLANNING_DASHBOARD")]
    public class PersonalAnnualPlanningDashboardView : IEntityMarker
    {
        public Guid Id { get; set; }
        public int YearPeriod { get; set; }
        public string NoReg { get; set; }
        public string DocumentNumber { get; set; }
        public string FormKey { get; set; }
        public string Title { get; set; }
        public int Progress { get; set; }
        public string DocumentStatusCode { get; set; }
        public string DocumentStatus { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
