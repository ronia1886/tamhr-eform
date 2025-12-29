using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BDJK_DETAIL")]
    public class BdjkDetail : IEntityBase<Guid>
    { 
        [Key]
        public Guid Id { get; set; }
        public string BdjkCode { get; set; }
        public string Description { get; set; }
        public string BdjkValue { get; set; }
        public string ClassFrom { get; set; }
        public string ClassTo { get; set; }
        public bool? FlagHoliday { get; set; }
        public int? FlagDuration { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool RowStatus { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

    }
}
