using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_OBJECT_DESCRIPTION", DatabaseObjectType.TableValued)]
    public class ObjectDescriptionStoredEntity
    {
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string Abbreviation { get; set; }
        public string OrgName { get; set; }
        public string ObjectDescription { get; set; }
    }
}
