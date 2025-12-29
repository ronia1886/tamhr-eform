
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_EMPLOYEE_INFO", DatabaseObjectType.StoredProcedure)]
    public class TerminationEmployeeInfoStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Division { get; set; }
        public string Position { get; set; }
        public string Class { get; set; }
        public string Job { get; set; }
    }
}
