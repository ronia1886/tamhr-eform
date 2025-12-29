using System;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DriverLicenseViewModel
    {
        public string SimType { get; set; }
        public string SimNumber { get; set; }
        public string NoReg { get; set; }
        public string Height { get; set; }
        public DateTime ValidityPeriod { get; set; }
        public bool RowStatus { get; set; }
    }
}
