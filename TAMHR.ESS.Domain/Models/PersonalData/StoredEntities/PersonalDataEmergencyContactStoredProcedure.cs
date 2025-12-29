using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_EMERGENCY_CONTACT", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataEmergencyContactStoredProcedure
    {
        public Guid Id { get; set; }
        public Guid CommonAttributeId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string EmergencyCallName { get; set; }
        public string EmergencyCallPhoneNumber { get; set; }
        public string EmergencyCallRelationshipCode { get; set; }
        public string EmergencyCallName2 { get; set; }
        public string EmergencyCallPhoneNumber2 { get; set; }
        public string EmergencyCallRelationshipCode2 { get; set; }
    }
}
