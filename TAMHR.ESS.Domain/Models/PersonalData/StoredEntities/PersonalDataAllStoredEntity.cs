using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_ALL_DATA", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataAllStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public Guid CommonAttributeId { get; set; }
        public bool? IsMainFamily { get; set; }
        public string FamilyTypeCode { get; set; }
        public string Name { get; set; }
        public string BirthPlace { get; set; }
        public DateTime BirthDate { get; set; }
        public string GenderCode { get; set; }
        public string BirthRegistration { get; set; }
        public string Condolance { get; set; }
        public string Marriage { get; set; }
        public string Divorce { get; set; }
        public DateTime? BirthRegistrationDate { get; set; }
        public DateTime? CondolanceDate { get; set; }
        public DateTime? MarriageDate { get; set; }
        public DateTime? DivorceDate { get; set; }
        public string BpjsNumber { get; set; }
        public string InsuranceNumber { get; set; }
        public int? Age { get; set; }
        public string PhoneNumber { get; set; }
        public string NIK { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string KKNumber { get; set; }
        public string Religion { get; set; }
        public string District { get; set; }
    }
}
