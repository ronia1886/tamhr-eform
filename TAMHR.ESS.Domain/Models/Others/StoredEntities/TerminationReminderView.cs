using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TERMINATION_REMINDER")]
    public class TerminationReminderView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string TerminationType { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string EmployeeName { get; set; }

    }
}
