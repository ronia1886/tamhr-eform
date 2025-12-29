using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_GENERAL_CATEGORY_MAPPING")]
    public partial class GeneralCategoryMapping : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string GeneralCategoryCode { get; set; }
        public string ParentGeneralCategoryCode { get; set; }
        public bool Readonly { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        //public GeneralCategory GeneralCategory { get; set; }
    }
}
