using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class WeeklyWFHPlanningViewModel
    {
        public WeeklyWFHPlanning WeeklyWFHPlanning { get; set; }
        public List<WeeklyWFHPlanningDetailView> WeeklyWFHPlanningDetails { get; set; }
        public string Remarks { get; set; }
    }
}
