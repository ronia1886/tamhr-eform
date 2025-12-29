using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    
    [Table("TB_M_SAP_GENERAL_CATEGORY_MAP")]
    public partial class SapGeneralCategoryMap : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string GeneralCategoryCode { get; set; }
        public string SapCode { get; set; }
        public string SapCategory { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
