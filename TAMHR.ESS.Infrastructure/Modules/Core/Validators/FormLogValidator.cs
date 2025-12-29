using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class FormLogValidator : AbstractValidator<FormLog>
    {
        public FormLogValidator(ConfigService configService, FormService formService, IStringLocalizer<FormLogValidator> localizer)
        {
            RuleFor(x => x.FormId).NotEmpty()
                .WithMessage(localizer["Must select form"].Value)
                .Must(x => formService.AnyFormId(x))
                .WithMessage(localizer["Form is not found"].Value);

            RuleFor(x => x.Notes).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0} characters"].Value, ApplicationConstants.VarcharMaxLength));
        }
    }
}
