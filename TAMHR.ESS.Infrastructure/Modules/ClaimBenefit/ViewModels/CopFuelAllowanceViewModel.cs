using System;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class CopFuelAllowanceViewModel
    {
        public List<DataCopFuelAllowanceViewModel> data;
        public string Remarks { get; set; }
    }
    public class DataCopFuelAllowanceViewModel
    {
        public DateTime? Date { get; set; }
        public string Destination { get; set; }
        public int Start { get; set; }
        public string IdStartAttachmentPath { get; set; }
        public string StartAttachmentPath { get; set; }
        public int Back { get; set; }
        public string IdBackAttachmentPath { get; set; }
        public string BackAttachmentPath { get; set; }
        public string Necessity { get; set; }
    }
}
