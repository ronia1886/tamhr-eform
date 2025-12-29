using FluentValidation;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PersonalDataOtherInformationValidator : AbstractDocumentValidator<OthersPersonalDataViewModel>
    {
        public PersonalDataOtherInformationValidator(IStringLocalizer<PersonalDataOtherInformationValidator> localizer)
        {
            RuleFor(x => x.Object.PhoneNumber1).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must(x => new Regex("^[0-9]+$").IsMatch(x))
                .WithMessage(localizer["Must be numeric value"].Value);

            When(x => !string.IsNullOrEmpty(x.Object.PhoneNumber2), () =>
            {
                RuleFor(x => x.Object.PhoneNumber2).Must(x => new Regex("^[0-9]+$").IsMatch(x ?? string.Empty))
                    .WithMessage(localizer["Must be numeric value"].Value);
            });

            When(x => !string.IsNullOrEmpty(x.Object.PhoneNumber3), () =>
            {
                RuleFor(x => x.Object.PhoneNumber3).Must(x => new Regex("^[0-9]+$").IsMatch(x ?? string.Empty))
                    .WithMessage(localizer["Must be numeric value"].Value);
            });

            RuleFor(x => x.Object.HomePhoneNumber).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must(x => new Regex("^[0-9]+$").IsMatch(x))
                .WithMessage(localizer["Must be numeric value"].Value);

            RuleFor(x => x.Object.PersonalEmail).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .EmailAddress()
                .WithMessage(localizer["Must be a valid email address"].Value);

            RuleFor(x => x.Object.Hobbies).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.Object.PublicTransportations).NotEmpty()
                .WithMessage(localizer["Must select a value for this field"].Value);

            RuleFor(x => x.Object.EmergencyCallRelationshipCode).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.Object.EmergencyCallRelationshipName).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.Object.EmergencyCallRelationshipPhoneNumber).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.Object.Remarks).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.Object.Confirmation).Must(x => x == "True")
                .WithMessage(localizer["Must accept this"].Value);
        }
    }
}
