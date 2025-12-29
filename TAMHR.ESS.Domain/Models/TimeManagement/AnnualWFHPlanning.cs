using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ANNUAL_WFH_PLANNING")]
    public partial class AnnualWFHPlanning : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string Submitter { get; set; }
        public int YearPeriod { get; set; }
        public int Version { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
