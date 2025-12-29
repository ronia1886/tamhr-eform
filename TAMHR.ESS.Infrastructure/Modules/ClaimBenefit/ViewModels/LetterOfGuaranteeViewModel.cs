using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class LetterOfGuaranteeViewModel
    {
        public string FamilyRelationship { get; set; }
        public Guid? FamilyRelationshipId { get; set; }
        public string PatientName { get; set; }
        public string PatientChildName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? StartDateOfCare { get; set; }
        public DateTime? EndDateOfCare { get; set; }
        public DateTime? ControlDate { get; set; }
        public string CriteriaControl { get; set; }
        public string Hospital { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalCity { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public string DoctorAgreementPath { get; set; }
        public string Diagnosa { get; set; }
        public string CheckUpCount { get; set; }
        public string DiagnosaRawatInap { get; set; }
        public string TreatmentResumePath { get; set; }
        public string Remarks { get; set; }
        public string BenefitClassification { get; set; }
    }
}
