using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    
    public class DpaRegisterViewModel
    {
        public string AccountType { get; set; }
        public string BankCode { get; set; }
        public string Branch { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string User { get; set; }
        public string Email { get; set; }
        public string HouseNumber { get; set; }
        public string Name { get; set; }
        public DateTime? BrithDate { get; set; }
        public string FamilyRelation { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<AhliWaris> AhliWaris { get; set; }
        public IEnumerable<AhliWaris> AhliWarisLama { get; set; }
    }

    public class AhliWaris
    {
        public string Name { get; set; }
        public DateTime? BrithDate { get; set; }
        public string FamilyRelation { get; set; }
        public string FamilyRelationCode { get; set; }
        public string GenderCode { get; set; }
    }
    public class AhliWarisLama
    {
        public string Name { get; set; }
        public DateTime? BrithDate { get; set; }
        public string FamilyRelation { get; set; }
        public string FamilyRelationCode { get; set; }
        public string GenderCode { get; set; }
    }
}
