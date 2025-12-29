using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AnnualWFHPlanningViewModel
    {
        public AnnualWFHPlanning AnnualWFHPlanning { get; set; }
        public List<AnnualWFHPlanningDetailView> AnnualWFHPlanningDetails { get; set; }
        public string Remarks { get; set; }
    }
}
