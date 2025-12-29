using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_EXIT_CLEARANCE_USERNAME", DatabaseObjectType.StoredProcedure)]
    public class TerminationExitClearanceUsernameStoredEntity
    {
        public string Username { get; set; }
        public string NoReg { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string RolePIC { get; set; }
        public Guid MatrixId { get; set; }
    }
}
