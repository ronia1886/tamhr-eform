using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BirthRegistrationValidator : AbstractDocumentValidator<BirthRegistrationViewModel>
    {
        public BirthRegistrationValidator()
        {
            When(x => !x.Object.IsDraft, () =>
            {

                RuleFor(x => x.Object.ChildName).NotEmpty();
                RuleFor(x => x.Object.DateOfBirth).NotEmpty();
                RuleFor(x => x.Object.NationalityCode).NotEmpty();
                RuleFor(x => x.Object.GenderCode).NotEmpty();
                RuleFor(x => x.Object.ReligionCode).NotEmpty();
                RuleFor(x => x.Object.BloodTypeCode).NotEmpty();
                RuleFor(x => x.Object.ChildStatus).NotEmpty();
                RuleFor(x => x.Object.BirthCertificatePath).NotEmpty();
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

            });
        }
    }
}
