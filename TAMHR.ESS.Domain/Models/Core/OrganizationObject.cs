using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_ORGANIZATION_OBJECT")]
    public partial class OrganizationObject : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public string ObjectID { get; set; }
        public string ObjectType { get; set; }
        public string Abbreviation { get; set; }
        public string ObjectText { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ObjectDescription { get; set; }
    }
}
