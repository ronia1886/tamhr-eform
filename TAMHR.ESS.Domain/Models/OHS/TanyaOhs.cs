using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TANYAOHS")]
    public partial class TanyaOhs : IEntityBase<Guid>
    {
        public Int64 RowNum { get; set; }
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string DoctorId { get; set; }
        public string Nama { get; set; }
        public string RealName { get; set; }
        public string Noreg { get; set; }
        public string Keluhan { get; set; }
        public string Solve { get; set; }
        public string Feedback { get; set; }
        public string Rating { get; set; }
        public string ReplyFeedback { get; set; }
        public string KategoriLayanan { get; set; }
        public string Status { get; set; }
        public int IsKonsultasiReadUser { get; set; }
        public int IsKonsultasiReadPic { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string CreatedOnSearch { get; set; }
    }
}
