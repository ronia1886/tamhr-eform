using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PersonalDataDocumentValidator : AbstractValidator<PersonalDataDocument>
    {
        public PersonalDataDocumentValidator(ConfigService configService, IStringLocalizer<PersonalDataDocumentValidator> localizer)
        {
            When(x => x.Id == Guid.Empty, () =>
            {
                RuleFor(x => x.NoReg).NotEmpty()
                    .WithMessage(localizer["Employee must be selected"]);

                RuleFor(x => x.DocumentTypeCode).NotEmpty()
                    .WithMessage(localizer["Please select document type"].Value)
                    .Must(x => configService.ValueInCategories(x, "PersonalDocumentType"))
                    .WithMessage(localizer["Value is not in category"].Value);
            });

            RuleFor(x => x.DocumentValue).NotEmpty()
                .WithMessage(localizer["Document value must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));
        }
    }
}
