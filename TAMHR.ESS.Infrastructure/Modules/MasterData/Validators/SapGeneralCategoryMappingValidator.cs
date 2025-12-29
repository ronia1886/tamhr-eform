using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class SapGeneralCategoryMappingValidator : AbstractValidator<SapGeneralCategoryMap>
    {
        public SapGeneralCategoryMappingValidator(ConfigService configService, IStringLocalizer<SapGeneralCategoryMappingValidator> localizer)
        {
            RuleFor(x => x.GeneralCategoryCode).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.SapCode).NotEmpty()
                .WithMessage(localizer["SAP code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.SapCategory).NotEmpty()
                .WithMessage(localizer["SAP category must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));
        }
    }
}
