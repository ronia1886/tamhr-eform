using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class FamilyMemberViewModel
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Gender { get; set; }
        public string FamilyType { get; set; }
        public string Other { get; set; }
        public string Bpjs { get; set; }
        public string InsuranceNumber { get; set; }
        public string ModifiedBy { get; set; }
        public int LifeStatus { get; set; }
        public DateTime? DeathDate {  get; set; }
        public string EducationLevel { get; set; }
        public string Job {  get; set; }
        public string PhoneNumber { get; set; }
        public string ChildStatus { get; set; }
        public string AddressStatusCode { get; set; }
        public string NIK { get; set; }
        public string Domicile { get; set; }
        public string Religion { get; set; }
        public string Nationality { get; set; }

    }
}
