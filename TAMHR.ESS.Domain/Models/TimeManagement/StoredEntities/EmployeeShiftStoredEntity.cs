using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMPLOYEE_SHIFT", DatabaseObjectType.TableValued)]
    public class EmployeeShiftStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string ShiftCode { get; set; }
    }
}
