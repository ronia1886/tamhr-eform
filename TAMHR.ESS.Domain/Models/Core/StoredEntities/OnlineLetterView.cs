using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ONLINE_LETTER")]
    public partial class OnlineLetterView : IEntityMarker
    {
        public Guid Id { get; set; }
        public DateTime LetterDate { get; set; } = DateTime.Now;
        public int LetterNumber { get; set; }
        public string Department { get; set; }
        public string PicTarget { get; set; }
        public string CompanyTarget { get; set; }
        public string LetterTypeCode { get; set; }
        public string LetterTypeName { get; set; }
        public bool Other { get; set; }
        public string Remarks { get; set; }
        public string Creator { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
