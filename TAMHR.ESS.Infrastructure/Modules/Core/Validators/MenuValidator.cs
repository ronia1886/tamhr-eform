using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MenuValidator : AbstractValidator<Menu>
    {
        public MenuValidator(ConfigService configService, IStringLocalizer<MenuValidator> localizer)
        {
            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value);

            RuleFor(x => x.MenuGroupCode).NotEmpty()
                .WithMessage(localizer["Please select menu group"].Value)
                .Must(x => configService.ValueInCategories(x, "MenuGroup"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.PermissionId).NotEmpty();

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonInputMaxLength));
        }
    }
}
