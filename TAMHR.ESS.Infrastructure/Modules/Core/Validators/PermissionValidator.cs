using FluentValidation;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PermissionValidator : AbstractValidator<Permission>
    {
        public PermissionValidator(ConfigService configService, IStringLocalizer<PermissionValidator> localizer)
        {
            RuleFor(x => x.PermissionKey).NotEmpty()
                .WithMessage(localizer["Role key must be filled"].Value);

            RuleFor(x => x.PermissionTypeCode).NotEmpty()
                .WithMessage(localizer["Please select permission type"].Value)
                .Must(x => configService.ValueInCategories(x, "PermissionType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));
        }
    }
}
