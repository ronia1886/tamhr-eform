
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_TYPE", DatabaseObjectType.StoredProcedure)]
    public class TerminationTypeStoredEntity
    {
        public Guid Id { get; set; }
        public string TerminationTypeKey { get; set; }
        public string TerminationTypeName { get; set; }
    }
}
