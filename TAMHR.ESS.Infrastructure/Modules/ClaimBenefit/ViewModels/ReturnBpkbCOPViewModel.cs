using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
   
    public class ReturnBpkbCOPViewModel
    {
        public Guid BpkpId { get; set; }
        public string BPKBNo { get; set; }
        public string PoliceNumber { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string ProductionYear { get; set; }
        public string Color { get; set; }
        public string FrameNumber { get; set; }
        public string MachineNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string NecessityCode { get; set; }
        public DateTime? LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Remarks { get; set; }
        public bool IsOtherNecessity { get; set; }
        public string OtherNecessity { get; set; }

    }
}
