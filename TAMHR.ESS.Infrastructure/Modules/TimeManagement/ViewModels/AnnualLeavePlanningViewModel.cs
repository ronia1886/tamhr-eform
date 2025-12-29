using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AnnualLeavePlanningViewModel
    {
        public AnnualLeavePlanning AnnualLeavePlanning { get; set; }
        public List<AnnualLeavePlanningDetailView> AnnualLeavePlanningDetails { get; set; }
        public int RemainingAnnualLeave { get; set; }
        public int RemainingLongLeave { get; set; }
        public string Remarks { get; set; }
        public int TotalLeave { get; set; }
        public int TotalAnnualLeave { get; set; }
        public int TotalLongLeave { get; set; }
        public int MaxLeaveDays { get; set; }
    }
}
