using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_ALLOWANCE_DETAIL")]
    public partial class AllowanceDetail : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public int ClassFrom { get; set; }
        public int ClassTo { get; set; }
        public decimal Ammount { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = new DateTime(9999, 12, 31);
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
