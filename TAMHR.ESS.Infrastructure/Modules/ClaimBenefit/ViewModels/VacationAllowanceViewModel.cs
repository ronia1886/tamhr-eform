using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class VacationAllowanceViewModel
    {
        public string AccountType { get; set; }
        public string AccountMore { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Location { get; set; }
        public string ProposalPath { get; set; }
        public DateTime? VacationDate { get; set; }
        public string Remarks { get; set; }
        public string User { get; set; }
        public IEnumerable<DepartmentVacation> Departments { get; set; }
        public IEnumerable<SummaryVacation> Summaries { get; set; }

        
        public string LocationActual { get; set; }
        public DateTime? VacationDateActual { get; set; }
        public int TotalParticipant { get; set; }
        public string AttachmentFilePath { get; set; }
        public Guid DocumentApprovalId { get; set; }
    }

    public class DepartmentVacation
    {
        public string ObjectID { get; set; }
        public IEnumerable<EmployeeVacation> Employees { get; set; }
    }

    public class EmployeeVacation
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public bool HasVacation { get; set; }
        //Kelas
        public int Classification { get; set; }
    }

    public class SummaryVacation
    {
        public string Range { get; set; }
        public int Qty { get; set; }
        public double Total { get; set; }
    }
}
