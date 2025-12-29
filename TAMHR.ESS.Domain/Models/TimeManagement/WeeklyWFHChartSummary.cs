using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    public class WeeklyWFHChartSummary
    {
        public string Name { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        //public int Percentage { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string WorkPlace { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Class { get; set; }
        public string MonthYear { get; set; }
        
        public string KeyDate { get; set; }

        public string FirstLoad { get; set; }

        public string employeeName { get; set; }
        public string planWorkPlace { get; set; }
        public string actualWorkPlace { get; set; }
    }
}
