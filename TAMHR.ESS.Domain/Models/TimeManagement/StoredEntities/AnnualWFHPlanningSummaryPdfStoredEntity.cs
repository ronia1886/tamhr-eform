
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_ANNUAL_WFH_PLANNING_SUMMARY_PDF", DatabaseObjectType.StoredProcedure)]
    public class AnnualWFHPlanningSummaryPdfStoredEntity
    {
        public string Category { get; set; }
        public string SuperiorNoReg { get; set; }
        public string SuperiorName { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int YearPeriod { get; set; }
        public int Jan { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Apr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Aug { get; set; }
        public int Sep { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dec { get; set; }
    }
}
