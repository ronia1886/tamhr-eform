using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class Loan36ViewModel
    {
        public int ClassRequester { get; set; }
        public bool IsVerified { get; set; }
        public string LoanType { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string SubDistrictCode { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public decimal? CostNeeds { get; set; }
        public int Interest { get; set; }
        public string CalculationLoan { get; set; }
        public decimal? BasicSalary { get; set; }
        public string Brand { get; set; }
        public decimal? LoanAmount { get; set; }
        public string SupportAttachmentPath { get; set; }
        public string Remarks { get; set; }
        public DateTime? Period { get; set; }
        public int Seq { get; set; }
        public int LoanPeriod { get; set; }


    }
}
