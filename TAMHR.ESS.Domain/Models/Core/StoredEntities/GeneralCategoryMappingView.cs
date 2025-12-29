using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GENERAL_CATEGORY_MAPPING")]
    public partial class GeneralCategoryMappingView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderSequence { get; set; }
        public bool RowStatus { get; set; }
        public Guid GeneralCategoryMappingId { get; set; }
        public string ParentGeneralCategoryCode { get; set; }
    }
}
