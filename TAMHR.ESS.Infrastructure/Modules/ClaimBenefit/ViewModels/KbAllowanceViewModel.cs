using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class KbAllowanceViewModel
    {
        public string FamilyRelationship { get; set; }
        public string PassienName { get; set; }
        public string Hospital { get; set; }
        public string HospitalAddress { get; set; }
        public DateTime? ActionKBDate { get; set; }
        public decimal Cost { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public string Remarks { get; set; }

    }
}
