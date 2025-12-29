using FluentValidation;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class FormValidator : AbstractValidator<Form>
    {
        public FormValidator(ConfigService configService, FormService formService, IStringLocalizer<FormValidator> localizer)
        {
            RuleFor(x => x.FormKey).NotEmpty()
                .WithMessage(localizer["Form key must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength))
                .Must((data, x) => !formService.AnyFormKey(data.Id, x))
                .WithMessage(data => string.Format(localizer[@"Form with key ""{0}"" already exist"].Value, data.FormKey));

            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.ModuleCode).NotEmpty()
                .WithMessage(localizer["Please select module"].Value)
                .Must(x => configService.ValueInCategories(x, "Module"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.TitleFormat).NotEmpty()
                .WithMessage(localizer["Title format must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.DocumentNumberFormat).NotEmpty()
                .WithMessage(localizer["Document number format must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.IsoNumber).NotEmpty()
                .WithMessage(localizer["ISO number must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallVarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallVarcharMaxLength));

            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);

            RuleFor(x => x.EndTime).GreaterThanOrEqualTo(x => x.StartTime)
                .WithMessage(localizer["End time must be greater than or equal to start time"].Value);
        }
    }
}
