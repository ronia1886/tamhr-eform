using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TERMINATION_REPORT_SUMMARY", DatabaseObjectType.TableValued)]
    public class TerminationReportSummaryStoredEntity
    {
        public Guid Id { get; set; }
        public string TerminationType { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
        public string DisplayName { get { return string.Format("{0} ({1})", Name, Total); } }
        //public string labelRetirement { get; set; }
        //public string labelPassed { get; set; }
        //public string labelResign { get; set; }
        //public string labelContract { get; set; }
        //public int totalRetirement { get; set; }
        //public int totalPassed { get; set; }
        //public int totalResign { get; set; }
        //public int totalContract { get; set; }

    }
}
