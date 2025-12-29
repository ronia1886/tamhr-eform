using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMPLOYEE_BY_CLASS_RANGE", DatabaseObjectType.TableValued)]
    public class EmployeeClassStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public int Staffing { get; set; }
    }
}
