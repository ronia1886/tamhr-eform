using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_COMMON_ATTRIBUTE")]
    public partial class PersonalDataCommonAttribute : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Nik { get; set; }
        public string KKNumber { get; set; }
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
        public string SubDistrictCode { get; set; }
        public string Rt { get; set; }
        public string Rw { get; set; }
        public string ReligionCode { get; set; }
        public string BloodTypeCode { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
