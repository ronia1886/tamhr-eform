using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_VACCINE_SUMMARY", DatabaseObjectType.TableValued)]
    public partial class VaccineSummaryStoredEntity
    {
        public string DivisionOrgCode { get; set; }
        public string Division { get; set; }
        public int Total { get; set; }
        public int TotalWorkFromOffice { get; set; }
        public int TotalWorkFromHome { get; set; }
        public int TotalNotSubmitted { get; set; }
    }
}
