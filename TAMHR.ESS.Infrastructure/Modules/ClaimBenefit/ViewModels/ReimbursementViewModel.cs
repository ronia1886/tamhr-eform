using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ReimbursementViewModel
    {
        public string FamilyRelationship { get; set; }
        public string PatientName { get; set; }
        public string PatientChildName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string NIK { get; set; }
        public string Hospital { get; set; }
        public string OtherHospital { get; set; }
        public bool IsOtherHospital { get; set; }
        public string HospitalAddress { get; set; }
        public DateTime? DateOfEntry { get; set; }
        public DateTime? DateOfOut { get; set; }
        public string InPatient { get; set; }
        public decimal Cost { get; set; }
        public string DetailOfCostPath { get; set; }
        public string ReceiptPath { get; set; }
        public string CopyOfPrescriptionPath { get; set; }
        public string ResumeMediaPath { get; set; }
        public string AnotherLetterPath { get; set; }
        public string Remarks { get; set; }
        public decimal TotalClaim { get; set; }
        public decimal TotalCompanyClaim { get; set; }
        public bool IsInputTotalClaim { get; set; } 
        public bool IsInputCompanyClaim { get; set; }
        public string AccountType { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ReimbursementPointViewModel
    {
        public Guid DocumentApprovalId { get; set; }
        public decimal TotalClaim { get; set; }
        public decimal TotalCompanyClaim { get; set; }
    }
}
