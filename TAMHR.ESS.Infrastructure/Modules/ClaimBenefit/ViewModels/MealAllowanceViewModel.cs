using System;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
   
    public class MealAllowanceViewModel
    {
        public DateTime? Period { get; set; }
        public decimal AmountTotal { get; set; }
        public List<DataMealAllowanceViewModel> data { get; set; }
        public string Remarks { get; set; }
        public string WageTypeCode { get; set; }
    }

    public class DataMealAllowanceViewModel
    {
        public TimeSpan? StartHour { get; set; }
        public TimeSpan? EndHour { get; set; }
        public decimal Amount { get; set; }
        public string IdSupportingAttachmentPath { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public DateTime? Date { get; set; }
    }
}
