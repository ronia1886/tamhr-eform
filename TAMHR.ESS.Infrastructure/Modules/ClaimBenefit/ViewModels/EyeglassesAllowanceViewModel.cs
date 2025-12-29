using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EyeglassesAllowanceViewModel
    {
        public string LensType { get; set; }
        public decimal AmountLens { get; set; }
        public string LensAttachment { get; set; }
        public DateTime? PurchaseLens { get; set; }
        public decimal AmountFrame { get; set; }
        public string FrameAttachment { get; set; }
        public DateTime? PurchaseFrame { get; set; }
        public string Remarks { get; set; }
        public bool IsLens { get; set; }
        public bool IsFrame { get; set; }

    }
}
