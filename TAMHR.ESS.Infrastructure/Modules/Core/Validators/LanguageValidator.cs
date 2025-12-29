using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class LanguageValidator : AbstractValidator<Language>
    {
        public LanguageValidator(CoreService coreService, ConfigService configService, IStringLocalizer<LanguageValidator> localizer)
        {
            RuleFor(x => x.CultureCode).NotEmpty()
                .WithMessage(localizer["Please select culture"].Value)
                .Must(x => configService.ValueInCategories(x, "Culture"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.TranslateKey).NotEmpty()
                .WithMessage(localizer["Translate key must be filled"].Value)
                .Must((data, x) => !coreService.CheckIfLangugeKeyExist(data.Id, data.CultureCode, x))
                .WithMessage(localizer["Translate with this key already exist please type another"].Value);

            RuleFor(x => x.TranslateValue).NotEmpty()
                .WithMessage(localizer["Translate value must be filled"].Value);
        }
    }
}
