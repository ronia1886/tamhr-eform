using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_EMPLOYEE_SUBGROUPNP")]
    public partial class EmployeeSubgroupNP : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string EmployeeSubgroup { get; set; }
        public int NP { get; set; }
        public int OrderNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
