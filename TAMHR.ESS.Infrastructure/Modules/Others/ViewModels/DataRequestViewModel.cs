using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DataRequestViewModel
    {
        public string DataDescription { get; set; }
        public string PurposeOfUsage { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string Remarks { get; set; }
        public string FilePath { get; set; }
    }
}
