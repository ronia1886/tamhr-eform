using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_MEAL_ALLOWANCE_SHIFT")]
    public partial class MealAllowanceShitf : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftCode { get; set; }
        public decimal Ammount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
