using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_COMMON_FILE")]
    public partial class CommonFile : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public static CommonFile Create(string fileName, int fileSize, string fileType, string fileUrl)
        {
            var commonFile = new CommonFile
            {
                FileName = fileName,
                FileSize = fileSize,
                FileType = fileType,
                FileUrl = fileUrl
            };

            return commonFile;
        }
    }
}
