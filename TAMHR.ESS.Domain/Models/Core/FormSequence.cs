using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM_SEQUENCE")]
    public partial class FormSequence : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public int Period { get; set; }
        public int SequenceNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static FormSequence Create(Guid formId, int period, int sequenceNumber)
        {
            var formSequence = new FormSequence
            {
                FormId = formId,
                Period = period,
                SequenceNumber = sequenceNumber
            };

            return formSequence;
        }
    }
}
