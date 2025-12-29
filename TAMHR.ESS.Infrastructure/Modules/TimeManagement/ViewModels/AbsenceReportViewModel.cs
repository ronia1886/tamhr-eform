using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AbsenceReportViewModel
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Approver { get; set; }

    }
}
