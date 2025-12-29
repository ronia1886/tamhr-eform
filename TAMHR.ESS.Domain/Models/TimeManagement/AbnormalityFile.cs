using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ABNORMALITY_FILE")]
    public partial class AbnormalityFile : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid CommonFileId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
