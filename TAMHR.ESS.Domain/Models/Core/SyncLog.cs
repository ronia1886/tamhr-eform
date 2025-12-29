using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_SYNC_LOG")]
    public partial class SyncLog : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string SyncTypeCode { get; set; }
        public string EventTypeCode { get; set; }
        public string MessageDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
