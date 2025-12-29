using FluentValidation;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MarriageStatusValidator : AbstractDocumentValidator<MarriageStatusViewModel>
    {
        public MarriageStatusValidator()
        {
            When(o => !o.Object.IsDraft, () =>
            {

                RuleFor(x => x.Object.NIK).NotEmpty().Matches(@"^\d{16}$").WithMessage("Identity Card Number must consist of 16 digits.");
                RuleFor(x => x.Object.KTPPath).NotEmpty();
                RuleFor(x => x.Object.PartnerName).NotEmpty();
                When(x => x.Object.IsOtherPlaceOfBirthCode.Equals(true), () =>
                {
                    RuleFor(x => x.Object.OtherPlaceOfBirthCode).NotEmpty();
                });
                When(x => x.Object.IsOtherPlaceOfBirthCode.Equals(false), () =>
                {
                    RuleFor(x => x.Object.PlaceOfBirthCode).NotEmpty();
                });
                When(x => x.Object.IsOtherNation.Equals(true), () =>
                {
                    RuleFor(x => x.Object.OtherNation).NotEmpty();
                });
                When(x => x.Object.IsOtherNation.Equals(false), () =>
                {
                    RuleFor(x => x.Object.NationCode).NotEmpty();
                });
                RuleFor(x => x.Object.DateOfBirth).NotEmpty();
                RuleFor(x => x.Object.SubDistrictCode).NotEmpty();
                RuleFor(x => x.Object.NationalityCode).NotEmpty();
                RuleFor(x => x.Object.GenderCode).NotEmpty();
                RuleFor(x => x.Object.ReligionCode).NotEmpty();
                RuleFor(x => x.Object.BloodTypeCode).NotEmpty();
                RuleFor(x => x.Object.ProvinceCode).NotEmpty();
                RuleFor(x => x.Object.CityCode).NotEmpty();
                RuleFor(x => x.Object.DistrictCode).NotEmpty();
                RuleFor(x => x.Object.PostalCode).NotEmpty();
                RuleFor(x => x.Object.Address).NotEmpty();
                RuleFor(x => x.Object.Job).NotEmpty();
                RuleFor(x => x.Object.MarriageDate).NotEmpty();
                RuleFor(x => x.Object.MarriageCertificatePath).NotEmpty();
                //RuleFor(x => x.Object.FamilyCardNo).NotEmpty();
                //RuleFor(x => x.Object.FamilyCardPath).NotEmpty();
                //RuleFor(x => x.Object.IsParnertBpjs).NotEmpty();
                When(x => x.Object.IsParnertBpjs.Equals(true), () =>
                {
                    RuleFor(x => x.Object.PartnerBjpsNo).NotEmpty();
                    RuleFor(x => x.Object.PartnerBjpsPath).NotEmpty();
                });
                When(x => x.Object.IsParnertBpjs.Equals(false), () =>
                {
                    RuleFor(x => x.Object.PartnerPhone).NotEmpty();
                    RuleFor(x => x.Object.PartnerEmail).NotEmpty().EmailAddress();
                    RuleFor(x => x.Object.FaskesCode).NotEmpty();
                    RuleFor(x => x.Object.FaskesName).NotEmpty();
                });


            });
            
        }
    }
}
