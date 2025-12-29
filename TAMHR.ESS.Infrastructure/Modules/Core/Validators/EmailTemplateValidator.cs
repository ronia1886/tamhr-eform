using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EmailTemplateValidator : AbstractValidator<EmailTemplate>
    {
        public EmailTemplateValidator(ConfigService configService, IStringLocalizer<EmailTemplateValidator> localizer)
        {
            RuleFor(x => x.ModuleCode).NotEmpty()
                .WithMessage(localizer["Please select module"].Value)
                .Must(x => configService.ValueInCategories(x, "Module"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.MailKey).NotEmpty()
                .WithMessage(localizer["Mail key must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.MailFrom).NotEmpty()
                .WithMessage(localizer["Mail from must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.DisplayName).NotEmpty()
                .WithMessage(localizer["Display name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.Subject).NotEmpty()
                .WithMessage(localizer["Subject must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));

            RuleFor(x => x.MailContent).NotEmpty()
                .WithMessage(localizer["Mail content must be filled"].Value);
        }
    }
}
