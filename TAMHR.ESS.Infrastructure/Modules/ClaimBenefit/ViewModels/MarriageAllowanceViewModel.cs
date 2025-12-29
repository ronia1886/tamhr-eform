using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MarriageAllowanceViewModel
    {
        public string PopulationNumber { get; set; }
        public string AmountAllowance { get; set; }
        public string PopulationPath { get; set; }
        public string PartnerName { get; set; }
        public string CountryCode { get; set; }
        public string OtherNation { get; set; }
        public string PlaceOfBirthCode { get; set; }
        public string OtherPlaceOfBirthCode { get; set; }
        public bool IsOtherPlaceOfBirthCode { get; set; }
        public bool IsOtherNation { get; set; }

        public string BloodTypeCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Citizenship { get; set; }
        public string GenderCode { get; set; }
        public string Religion { get; set; }
        public DateTime? WeddingDate { get; set; }
        public string MarriageCertificatePath { get; set; }
        public string Remarks { get; set; }

    }
}
