using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class SpklReportViewModel
    {
        public Guid CommonnId { get; set; }
        public string Type { get; set; }
        public string ActionType { get; set; }
        public string Noreg { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
