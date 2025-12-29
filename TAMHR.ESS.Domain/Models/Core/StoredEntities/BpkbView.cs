using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_BPKB")]
    public partial class BpkbView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string RequestType { get; set; }
        public string NoBPKB { get; set; }
        public string LicensePlat { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string CreatedYear { get; set; }
        public string Color { get; set; }
        public string VINNo { get; set; }
        public string EngineNo { get; set; }
        public string VehicleOwner { get; set; }
        public string Address { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
