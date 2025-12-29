using System;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class VaccineViewModel
    {
        public VaccineViewModel()
        {
            FormAnswers = new List<VaccineAnswerViewModel>();
        }
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string FamilyStatus { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Domicile { get; set; }
        public string Address { get; set; }
        public string SubDistrict { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string IdentityId { get; set; }
        public string IdentityImage { get; set; }
        public string Allergies { get; set; }
        public DateTime? LastNegativeSwabDate { get; set; }
        public bool IsPregnant { get; set; }
        public string OtherQuestion { get; set; }
        public bool OtherVaccine { get; set; }
        public bool VaccineAgreement { get; set; }
        public DateTime? VaccineDate1 { get; set; }
        public string VaccineHospital1 { get; set; }
        public string VaccineCard1 { get; set; }
        public string VaccineType1 { get; set; }
        public DateTime? VaccineDate2 { get; set; }
        public string VaccineHospital2 { get; set; }
        public string VaccineCard2 { get; set; }
        public string VaccineType2 { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public List<VaccineAnswerViewModel> FormAnswers { get; set; }
    }

    public class VaccineAnswerViewModel
    {
        public VaccineAnswerViewModel()
        {
            FormAnswerDetails = new List<VaccineAnswerDetailViewModel>();
        }
        public Guid Id { get; set; }
        public Guid FormQuestionId { get; set; }
        public Guid VaccineId { get; set; }
        public string Answer { get; set; }
        public List<VaccineAnswerDetailViewModel> FormAnswerDetails { get; set; }
    }

    public class VaccineAnswerDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid VaccineQuestionId { get; set; }
        public string Answer { get; set; }
    }
}
