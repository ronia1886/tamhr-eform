using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [Table("VW_VACCINE_SCHEDULE_DOWNLOAD")]
    
    public partial class VaccineScheduleDownloadView : IEntityMarker
    {
        [Key]
        public int VaccineNumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Hospital { get; set; }
        public DateTime VaccineDate { get; set; }
        public int Qty { get; set; }
        public int RemainingQty { get; set; }
    }
}
