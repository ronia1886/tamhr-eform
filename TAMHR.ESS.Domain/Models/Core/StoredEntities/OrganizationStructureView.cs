using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ORGANIZATION_STRUCTURE")]
    public partial class OrganizationStructureView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string ParentOrgCode { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
    }
}
