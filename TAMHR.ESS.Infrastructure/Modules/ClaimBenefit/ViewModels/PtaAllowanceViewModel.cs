using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
      
    public class PtaAllowanceViewModel
    {
        public string CateogryPTA { get; set; }
        public DateTime? date { get; set; }
        public decimal Amouont { get; set; }
        public string ProposalPath { get; set; }
        public string AccountType { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string User { get; set; }

        public string Summary { get; set; }
        public string DetailNote { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<DepartmentPta> Departments { get; set; }
        public IEnumerable<SummaryPta> Summaries { get; set; }
        //public IEnumerable<EmployeePta> Employees { get; set; }
    }

    public class DepartmentPta
    {
        public string ObjectID { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
        public IEnumerable<EmployeePta> Employees { get; set; }
    }

    public class EmployeePta
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgCode { get; set; }
        public string Reward { get; set; } 
        //Kelas
        public int Classification { get; set; }
    }

    public class SummaryPta
    {
        public string Range { get; set; }
        public string ClassFrom { get; set; }
        public string ClassTo { get; set; }
        public string Reward { get; set; }
        public string RewardText { get; set; }
        public int Qty { get; set; }
        public double Amount { get; set; }
        public double Total { get; set; }
        public DateTime EventDate { get; set; }
        public IEnumerable<EmployeePta> Employees { get; set; }
    }
}
