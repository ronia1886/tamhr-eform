using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_ALL_PROFILE_ATTRIBUTE", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataAllProfileStoredEntity
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
        public string AdministrativeVillage { get; set; }
        public string ResidentialStatus { get; set; }
        public string Domicile { get; set; }
        public string DomicilePostalCode { get; set; }
        public string DomicileRegion { get; set; }
        public string DomicileDistrict { get; set; }
        public string DomicileSubDistrict { get; set; }
        public string DomicileRt { get; set; }
        public string DomicileRw { get; set; }
        public string DomicileAdministrativeVillage { get; set; }
        public string DomicileResidentialStatus { get; set; }
        public string DomicileTotalFamily { get; set; }
        public string Religion { get; set; }
        public string BloodType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Npwp { get; set; }
        public string TaxStatus { get; set; }

        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }

        public string BpjsNumber { get; set; }
        public string FaskesName { get; set; }
        public string InsuranceNumber { get; set; }
        public string BpjsKetenagakerjaan { get; set; }
        public string DanaPensiunAstra { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string IdentityCardName { get; set; }
        public string SimANumber { get; set; }
        public string SimCNumber { get; set; }
        public string PassportNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string WhatsappNumber {  get; set; }
        public DateTime? MarriedDate { get; set; }
        public DateTime? DivorceDate { get; set; }
        public string MarriedCertificate { get; set; } = string.Empty;
        public string DivorceCertificate { get; set; } = string.Empty;
    }
}
