using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class TerminationReportViewModel
    {
        //private TerminationReportSummaryStoredEntity data;

        //public TerminationReportViewModel(TerminationReportSummaryStoredEntity data)
        //{
        //    this.data = data;
        //}

        public string Code { get; }
        public string Name { get; }
        public int Total { get; set; }
        public string DisplayName { get { return string.Format("{0} ({1})", Name, Total); } }
        public string UserColor { get; set; }
        //public string labelRetirement { get; set; }
        //public string labelPassed { get; set; }
        //public string labelResign { get; set; }
        //public string labelContract { get; set; }

        //public int totalRetirement { get; set; }
        //public int totalPassed { get; set; }
        //public int totalResign { get; set; }
        //public int totalContract { get; set; }

    }
}
