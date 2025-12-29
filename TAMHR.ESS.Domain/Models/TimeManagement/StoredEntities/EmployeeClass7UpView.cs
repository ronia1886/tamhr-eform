using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_EMPLOYEE_CLASS7UP")]
    public class EmployeeClass7UpView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string EmployeeSubGroup { get; set; }
        public int OrgLevel { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public int Class { get; set; }
        public string SuperiorNoReg { get; set; }
    }
}
