using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TERMINATION_REPORT")]
    public class TerminationReportView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string Class { get; set; }
        public string JobName { get; set; }
        public string Reason { get; set; }
        public string Terminationtype { get; set; }
        public string TerminationName { get; set; }
        public string DocumentStatus { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        
    }
}
