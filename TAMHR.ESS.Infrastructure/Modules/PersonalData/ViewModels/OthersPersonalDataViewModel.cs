using System;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class OthersPersonalDataViewModel
    {
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string PhoneNumber3 { get; set; }
        public string HomePhoneNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string Hobbies { get; set; }
        public string[] PublicTransportations { get; set; }
        public string Remarks { get; set; }
        public string EmergencyCallRelationshipCode { get; set; }
        public string EmergencyCallRelationshipName { get; set; }
        public string EmergencyCallRelationshipPhoneNumber { get; set; }
        public string Confirmation { get; set; }
    }
}
