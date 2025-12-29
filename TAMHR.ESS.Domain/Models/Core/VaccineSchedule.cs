using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_VACCINE_SCHEDULE")]
    public partial class VaccineSchedule : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int? VaccineNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
      
    }
    public class totalSheetEditGridViewModel
    {
        public IEnumerable<TotalSheetRowViewModel> Rows { get; set; }
        public IEnumerable<TotalSheetColumnViewModel> Columns { get; set; }
    }

    public class TotalSheetRowViewModel
    {

        public int RowNo { get; set; }
        public string Description { get; set; }
        public double?[] Totals { get; set; }

    }

    public class TotalSheetColumnViewModel { public int Id { get; set; } public string Title { get; set; } }
}
