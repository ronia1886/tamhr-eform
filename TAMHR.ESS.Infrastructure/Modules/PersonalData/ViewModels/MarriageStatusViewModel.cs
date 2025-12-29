using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MarriageStatusViewModel
    {
        public bool IsDraft { get; set; }
        public string NIK { get; set; }
        public string KTPPath { get; set; }
        public string PartnerName { get; set; }
        public string AmountAllowance { get; set; }
        public string PartnerNameId { get; set; }
        public string NationCode { get; set; }
        public string OtherNation { get; set; }
        public bool IsOtherPlaceOfBirthCode { get; set; }
        public bool IsOtherNation { get; set; }
        public string OtherPlaceOfBirthCode { get; set; }
        public string PlaceOfBirthCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NationalityCode { get; set; }
        public string GenderCode { get; set; }
        public string ReligionCode { get; set; }
        public string BloodTypeCode { get; set; }
        public string ProvinceCode { get; set; }
        public string CityCode { get; set; }
        public string DistrictCode { get; set; }
        public string SubDistrictCode { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string Rt { get; set; }
        public string Rw { get; set; }
        public string Job { get; set; }
        public DateTime? MarriageDate { get; set; }
        public string MarriageCertificatePath { get; set; }
        public string FamilyCardNo { get; set; }
        public string FamilyCardPath { get; set; }
        public bool IsParnertBpjs { get; set; }
        public string FaskesCode { get; set; }
        public string FaskesName { get; set; }
        public string PartnerPhone { get; set; }
        public string PartnerEmail { get; set; }
        public string PartnerBjpsNo { get; set; }
        public string Remarks { get; set; }
        public string PartnerBjpsPath { get; set; }


        //public List<DocumentApprovalFile> attachments { get; set; }
    }
}
