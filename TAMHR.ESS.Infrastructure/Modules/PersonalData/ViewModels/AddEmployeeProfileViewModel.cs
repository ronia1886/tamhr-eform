using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AddEmployeeProfileViewModel
    {
        public string Noreg { get; set; }
        public string Employee { get; set; }
        public DateTime EntryDate { get; set; }
        public string EmployeeCategory { get; set; }
        public string EmployeeStatus { get; set; }
        public string PersArea { get; set; }
        public string SubArea { get; set; }
        public string Class { get; set; }
        public string Nationality { get; set; }
        public string Pob { get; set; }
        public DateTime Dob { get; set; }
        public string GenderCode { get; set; }
        public string Idn { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NPWP { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string Religion { get; set; }
        public string BloodGroup { get; set; }
        public string AccountNumber { get; set; }
        public string Branch { get; set; }
        public string TaxStatus { get; set; }
        public string BPJS { get; set; }
        public string BPJSEmployment { get; set; }
        public string Insuranceno { get; set; }
        public string Nodanapensiun { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string IdentityCardName { get; set; }
        public DateTime AstraDate { get; set; }
        public string PassportNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string WhatsappNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string Domicile { get; set; }
        public string SimANumber { get; set; }
        public string SimCNumber { get; set; }
    }
    public class AddEducationProfile
    {
        public string NoregEducation { get; set; }
        public string EmployeeEducation { get; set; }
        public string Education { get; set; }
        public string NameEducation { get; set; }
        public string Major { get; set; }
        public string Country { get; set; }
    }
    public class AddFamilyProfile
    {
        public string NoregFamily { get; set; }
        public string EmployeeFamily { get; set; }
        public string FamilyType { get; set; }
        public string NameFamily { get; set; }
        public string FamilyPob { get; set; }
        public DateTime FamilyDob { get; set; }
        public string GenderFamily { get; set; }
        public string FamilyBPJS { get; set; }
        public string FamilyInsuranceNo { get; set; }
        public string FamilyIdentityNumber { get; set; }
        public int? LifeStatus { get; set; }
        public DateTime FamilyDod {  get; set; }
        public string ChildStatus { get; set; }
        public string ChildOrder {  get; set; }
        public string FamilyPhoneNumber { get; set; }
        public string FamilyDomicile {  get; set; }
        public string FamilyEdu { get; set; }
        public string FamilyJob { get; set; }
    }
}
