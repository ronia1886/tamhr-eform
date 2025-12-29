using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class CopExplorationBpkbViewModel
    {
        public string PoliceNumber { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string ProductionYear { get; set; }
        public string Color { get; set; }
        public string FrameNumber { get; set; }
        public string MachineNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Remarks { get; set; }

    }
}
