using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    public class WeeklyWFHReports
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Plan { get; set; }
        public string Actual { get; set; }
        public string Division { get; set; }
        public string Departement { get; set; }
        public string Section { get; set; }
        public string NoReg { get; set; }
    }
}
