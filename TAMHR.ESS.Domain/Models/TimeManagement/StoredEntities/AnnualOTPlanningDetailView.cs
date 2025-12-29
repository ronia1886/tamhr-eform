using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ANNUAL_OT_PLANNING_DETAIL")]
    public class AnnualOTPlanningDetailView : IEntityMarker
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid DocumentApprovalId { get; set; }
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
        public string Total { get; set; }
        public int OrderSequence { get; set; }
    }
}
