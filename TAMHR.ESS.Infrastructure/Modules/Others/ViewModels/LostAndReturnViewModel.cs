using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class LostAndReturnViewModel
    {
        public string CategoryCode { get; set; }
        public string DocumentCategoryCode { get; set; }
        public string Remarks { get; set; }
        public DateTime? LostDate { get; set; }
        public string DamagedRemarks { get; set; }
        public string Location { get; set; }
        public string FilePath { get; set; }
    }
}
