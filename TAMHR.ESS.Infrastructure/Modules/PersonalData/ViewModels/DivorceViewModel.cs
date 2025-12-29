using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DivorceViewModel
    {
        public bool IsDraft { get; set; }
        public string PartnerName { get; set; }
        public string PartnerId { get; set; }
        public DateTime? DivorceDate { get; set; }
        public string DivorceCertificatePath { get; set; }
        public string Remarks { get; set; }

    }
}
