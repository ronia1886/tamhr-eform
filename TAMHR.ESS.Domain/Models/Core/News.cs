using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_NEWS")]
    public partial class News : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string SlugUrl { get; set; }
        public string BodyHtml { get; set; }
        public string ShortDescription { get; set; }
        public int OrderIndex { get; set; } = 1;
        public bool Sticky { get; set; }
        public string ImageUrl { get; set; }
        public int ViewCount { get; set; }
        public int ReadCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
