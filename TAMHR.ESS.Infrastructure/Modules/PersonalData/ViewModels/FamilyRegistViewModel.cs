using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class FamilyRegistViewModel
    {
        public Guid Id { get; set; }
        public bool IsDraft { get; set; }
        public string ChildName { get; set; }
        public string NationCode { get; set; }
        public string OtherNation { get; set; }
        public string PlaceOfBirthCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalityCode { get; set; }
        public string GenderCode { get; set; }
        public string ReligionCode { get; set; }
        public string BloodTypeCode { get; set; }
        public string ChildStatus { get; set; }
        public string BirthCertificatePath { get; set; }
        public string Remarks { get; set; }
        public bool IsOtherPlaceOfBirthCode { get; set; }
        public bool IsOtherNation { get; set; }
        public string OtherPlaceOfBirthCode { get; set; }
        public string AnakKe { get; set; }
        public string Name { get; set; }
        public string Provinsi { get; set; }
        public string Kota { get; set; }
        public string Kecamatan { get; set; }
        public string PostalCode { get; set; }
        public string Kelurahan { get; set; }
        public string CompleteAddress { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string Nik { get; set; }
        public string KKNumber { get; set; }
        public string Address { get; set; }
    }
}
