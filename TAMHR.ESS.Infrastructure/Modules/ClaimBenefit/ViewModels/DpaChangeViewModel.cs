using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    
    public class DpaChangeViewModel
    {
        public bool? AccountType { get; set; }
        public string AccountMore { get; set; }
        public string BankCode { get; set; }
        public string Branch { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string HouseNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime? BrithDate { get; set; }
        public DateTime? DateOfOut { get; set; }
        public string FamilyRelation { get; set; }
        public string InPatient { get; set; }
        public string Cost { get; set; }
        public string DetailOfCostPath { get; set; }
        public string ReceiptPath { get; set; }
        public string CopyOfPrescriptionPath { get; set; }
        public string ResumeMediaPath { get; set; }
        public string AnotherLetterPath { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<AhliWaris> AhliWaris { get; set; }
    }

}
