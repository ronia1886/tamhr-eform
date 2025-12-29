using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ABNORMALITY_FILE")]
    public class AbnormalityFileView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid? CommonFileId { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? FileSize { get; set; }
        public string FileType { get; set; }
        public bool? RowStatus { get; set; }
        public string FileUrl { get; set; }
    }
}
