using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    
    public class ShiftMealAllowanceViewModel
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string StartPeriod { get; set; }
        public string EndPeriod { get; set; }
        public string ShiftType { get; set; }
        public string Remarks { get; set; }

        public IEnumerable<ShiftMealAllowanceSummaryViewModel> Summaries { get; set; }

    }

    public class PeriodeShiftMeal
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class ShiftMealAllowanceSummaryViewModel
    {
        public DateTime Date { get; set; }

        public int Qty { get; set; }

        public decimal Total { get; set; }

        public IEnumerable<ShiftMealAllowanceEmployeeViewModel> Employees { get; set; }

    }

    public class ShiftMealAllowanceEmployeeViewModel
    {
        public string NoReg { get; set; }

        public string Name { get; set; }

        //Kelas
        public int Classification { get; set; }

        public decimal Amount { get; set; }

        public string ShiftCode { get; set; }


    }
}
