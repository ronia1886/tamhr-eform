using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_DIVISION_PLAN")]
    public class WorkDivisionPlan : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string OrgCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public GeneralCategory Plan { get; set; }
        public OrganizationObject Division { get; set; }
    }
}
