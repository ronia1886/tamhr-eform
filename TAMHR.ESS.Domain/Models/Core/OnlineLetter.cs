using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ONLINE_LETTER")]
    public partial class OnlineLetter : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? LetterDate { get; set; }
        public int? LetterNumber { get; set; }
        public string Department { get; set; }
        public string PicTarget { get; set; }
        public string CompanyTarget { get; set; }
        public string LetterTypeCode { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
