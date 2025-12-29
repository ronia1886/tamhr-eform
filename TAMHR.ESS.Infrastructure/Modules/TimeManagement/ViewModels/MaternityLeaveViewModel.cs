using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MaternityLeaveViewModel
    {
        public string GestationalAge { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? EstimatedDayOfBirth { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public DateTime? StartHandoverOfWork { get; set; }
        public DateTime? StartMaternityLeave { get; set; }
        public DateTime? BackToWork { get; set; }
        public string MedicalMertificatePath { get; set; }
        public string BirthCertificatePath { get; set; }
        public string Remarks { get; set; }
    }

    public class MaternityLeaveDobViewModel
    {
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public DateTime BackToWork { get { return (DayOfBirth ?? DateTime.Now).AddDays(45); } }
        public string BirthCertificatePath { get; set; }
        public IEnumerable<DocumentApprovalFile> Attachments { get; set; }
    }
}
