using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AnnualBDJKPlanningViewModel
    {
        public AnnualBDJKPlanning AnnualBDJKPlanning { get; set; }
        public List<AnnualBDJKPlanningDetailView> AnnualBDJKPlanningDetails { get; set; }
        public string Remarks { get; set; }
    }
}
