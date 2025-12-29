using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EMPLOYEE_SUBSTITUTION")]
    public class EmployeeSubstitutionView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime Date { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftCodeUpdate { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
    }
}
