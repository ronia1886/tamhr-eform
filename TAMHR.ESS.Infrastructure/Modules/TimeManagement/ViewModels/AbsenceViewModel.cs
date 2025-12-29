using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AbsenceViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ReasonId { get; set; }
        public string Reason { get; set; }
        public string ReasonType { get; set; }
        public string Description { get; set; }
        public string TotalAbsence { get; set; }
        public int? RemainingDaysOff { get; set; }
        public int? RemainingLongLeave { get; set; }
        public bool? MandatoryAttachment { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public Nullable<bool> IsPlanning { get; set; }
        public string Remarks { get; set; }
        public Guid? AnnualLeavePlanningDetailId { get; set; }
        public string KategoriPenyakit { get; set; }
        public string SpesifikPenyakit { get; set; }
    }
}
