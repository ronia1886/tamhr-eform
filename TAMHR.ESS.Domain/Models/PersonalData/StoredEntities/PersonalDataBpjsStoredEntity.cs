using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_BPJS_DATA", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataBpjsStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string EmployeeName { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string BpjsNumber { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FaskesCode { get; set; }
        public string FaskesCity { get; set; }
        public string FaskesName { get; set; }
        public string FaskesAddress { get; set; }
        public string FamilyTypeCode { get; set; }
        public bool? IsMainFamily { get; set; }
        public string Name { get; set; }
        public string Nik { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string NationalityCode { get; set; }
        public string GenderCode { get; set; }
        public string CountryOfBirthCode { get; set; }
        public string Address { get; set; }
        public string CityCode { get; set; }
        public string PostalCode { get; set; }
        public string RegionCode { get; set; }
        public string CountryCode { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string SubDistrictCode { get; set; }
        public string SubDistrictName { get; set; }
        public string Rt { get; set; }
        public string Rw { get; set; }
        public string ReligionCode { get; set; }
        public string BloodTypeCode { get; set; }
        public string ActionType { get; set; }
        public bool CompleteStatus { get; set; }
        public string FamilyRelation { get; set; }
        public string FamilyName { get; set; }
        public string KKNumber { get; set; }
        public string FamilyRelationNum { get; set; }
        public string FamilyGenderCode { get; set; }
    }
}
