
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_PDF_EMPLOYEE_INFO", DatabaseObjectType.StoredProcedure)]
    public class TerminationPdfEmployeeInfoStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
        public string Class { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime StartDate { get; set; }
        public string Position { get; set; }
        public string Salutation { get; set; }
    }
}
