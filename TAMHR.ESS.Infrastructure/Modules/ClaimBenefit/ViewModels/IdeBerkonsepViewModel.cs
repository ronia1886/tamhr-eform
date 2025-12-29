using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class IdeBerkonsepViewModel
    {
        public string CriretiaCode { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public decimal Amount { get; set; }
        public string ProposalPath { get; set; }
        public string Remarks { get; set; }

    }
}
