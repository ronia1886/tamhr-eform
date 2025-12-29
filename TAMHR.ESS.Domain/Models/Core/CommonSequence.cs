using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_COMMON_SEQUENCE")]
    public partial class CommonSequence : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string SequenceKey { get; set; }
        public int SequenceNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static CommonSequence Create(string sequenceKey, int sequenceNumber)
        {
            var commonSequence = new CommonSequence
            {
                SequenceKey = sequenceKey,
                SequenceNumber = sequenceNumber
            };

            return commonSequence;
        }
    }
}
