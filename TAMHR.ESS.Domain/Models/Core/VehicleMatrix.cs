using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    
    [Table("TB_M_VEHICLE_MATRIX")]
    public partial class VehicleMatrix : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string Class { get; set; }
        public int GroupSequence { get; set; }
        public string SequenceClass { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string Position { get; set; }

        public bool? IsUpgrade { get; set; }
        
    }
}
