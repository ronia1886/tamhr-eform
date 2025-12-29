using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MarriageAllowanceValidator : AbstractDocumentValidator<MarriageAllowanceViewModel>
    {
        public MarriageAllowanceValidator()
        {
            RuleFor(x => x.Object.PopulationNumber).NotEmpty().Matches(@"^\d{16}$").WithMessage("Identity Card Number must consist of 16 digits.");
            RuleFor(x => x.Object.PopulationPath).NotEmpty();
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
                RuleFor(x => x.Object.CountryCode).NotEmpty();
            });
            RuleFor(x => x.Object.BloodTypeCode).NotEmpty();
            RuleFor(x => x.Object.DateOfBirth).NotEmpty();
            RuleFor(x => x.Object.Citizenship).NotEmpty();
            RuleFor(x => x.Object.GenderCode).NotEmpty();
            RuleFor(x => x.Object.Religion).NotEmpty();
            RuleFor(x => x.Object.WeddingDate).NotEmpty();
            RuleFor(x => x.Object.MarriageCertificatePath).NotEmpty();
        }
    }
}
