using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AnnualOTPlanningViewModel
    {
        public AnnualOTPlanning AnnualOTPlanning { get; set; }
        public List<AnnualOTPlanningDetailView> AnnualOTPlanningDetails { get; set; }
        public string Remarks { get; set; }
    }
}
