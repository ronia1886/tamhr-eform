using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DistressedAllowanceViewModel
    {
        public string Description { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public DateTime? DateDistressed { get; set; }
        public string AmountAllowance { get; set; }
        public string Remarks { get; set; }

    }
}
