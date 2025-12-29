using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMP_ORGANIZATION_OBJECT", DatabaseObjectType.StoredProcedure)]
    public class EmployeeeOrganizationObjectStoredEntity
    {
        public Guid Id { get; set; }
        public string ObjectID { get; set; }
        public string ObjectType { get; set; }
        public string Abbreviation { get; set; }
        public string ObjectText { get; set; }
        public DateTime EntryDate { get; set; }
        public string NP { get; set; }
        public string Kelas { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EmployeeSubGroupText { get; set; }
        public string ObjectDescription { get; set; }
        public string PersonnelArea { get; set; }
        public string CostCenter { get; set; }
        public decimal Staffing { get; set; }
    }
}
