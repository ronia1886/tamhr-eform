using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{ 
    [Table("TB_M_FORM_VALIDATION_MATRIX")]
    public partial class FormValidationMatrix : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        public Guid FormId { get; set; }
        public string RequestType { get; set; }
        public string SubRequestType { get; set; }
        public string FromClass { get; set; }
        public string ToClass { get; set; }
        public int? PeriodDay { get; set; }
        public int? PeriodMonth { get; set; }
        public int? PeriodYear { get; set; }
        public string AdditionalFlag { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [NotMapped]
        public string IconClass { get; set; }
    }
}
