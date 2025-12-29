using Agit.Common.Attributes;
using System;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ANNUAL_PLANNING_SUBORDINATE_DASHBOARD", DatabaseObjectType.TableValued)]
    public class AnnualPlanningGetSubordinateStoredEntity
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
        public string SuperiorNoReg { get; set; }
    }
}
