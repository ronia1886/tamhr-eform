using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class TaxStatusViewModel
    {
        public string NPWPNumber { get; set; }
        public string StatusTax { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public string Remarks { get; set; }

    }
}
