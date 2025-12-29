using Agit.Common.Attributes;
using System;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_SPKL_DETAIL_DOCUMENT_NUMBER", DatabaseObjectType.TableValued)]
    public class SpklRequestDetailDocumentNumberStoredEntity
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
    }
}
