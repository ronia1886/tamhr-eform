using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class NotebookRequestViewModel
    {
        public string AssetNumber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddYears(1);
        public string Reason { get; set; }
        public string Remarks { get; set; }
        public string FilePath { get; set; }
    }
}
