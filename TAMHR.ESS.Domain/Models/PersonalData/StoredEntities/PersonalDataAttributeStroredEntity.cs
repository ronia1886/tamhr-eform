using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_PROFILE_ATTRIBUTE", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataAttributeStroredEntity
    {
        public Guid Id { get; set; }
        public Guid CommonAttributeId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Nik { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string MartalStatus { get; set; }
        public string KKNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string CountryOfBirth { get; set; }
        public string Address { get; set; }
        public string CityCode { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string SubDistrict { get; set; }
        public string Rt { get; set; }
        public string Rw { get; set; }
        public string Religion { get; set; }
        public string BloodType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Npwp { get; set; }
        public string TaxStatus { get; set; }

        public string AccountNumber { get; set; }
        public string BankName { get; set; }

        public string BpjsNumber { get; set; }
        public string FaskesName { get; set; }
        public string InsuranceNumber { get; set; }
        public string BpjsKetenagakerjaan { get; set; }
        public string DanaPensiunAstra { get; set; }
        public string EmployeeSubgroupText { get; set; }
    }
}
