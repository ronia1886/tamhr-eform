using System;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class PassportViewModel
    {
        public string PassportNumber { get; set; }
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public string Office { get; set; }

        public string NoReg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
