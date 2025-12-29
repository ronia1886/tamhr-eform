using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ConsecutiveLeavePlanningViewModel
    {
        public AnnualLeavePlanningDetailView NewLeave { get; set; }
        public List<AnnualLeavePlanningDetailView> Details { get; set; }
    }
}
