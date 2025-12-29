
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_INITIAL_ANNUAL_OT", DatabaseObjectType.StoredProcedure)]
    public class AnnualOTPlanningDetailInitial
    {
        public string Division { get; set; }
        public string CategoryCode { get; set; }
        public string Category { get; set; }
        public string LabourType { get; set; }
        public string Jan { get; set; }
        public string Feb { get; set; }
        public string Mar { get; set; }
        public string Apr { get; set; }
        public string May { get; set; }
        public string Jun { get; set; }
        public string Jul { get; set; }
        public string Aug { get; set; }
        public string Sep { get; set; }
        public string Oct { get; set; }
        public string Nov { get; set; }
        public string Dec { get; set; }
        public int OrderSequence { get; set; }
    }
}
