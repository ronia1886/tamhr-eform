using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class RoleValidator : AbstractValidator<Role>
    {
        public RoleValidator(ConfigService configService, IStringLocalizer<RoleValidator> localizer)
        {
            RuleFor(x => x.RoleKey).NotEmpty()
                .WithMessage(localizer["Role key must be filled"].Value);

            RuleFor(x => x.RoleTypeCode).NotEmpty()
                .WithMessage(localizer["Please select role type"].Value)
                .Must(x => configService.ValueInCategories(x, "RoleType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));
        }
    }
}
