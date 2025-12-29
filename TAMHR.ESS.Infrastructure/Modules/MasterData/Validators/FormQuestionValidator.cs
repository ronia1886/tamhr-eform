using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class FormQuestionValidator : AbstractValidator<FormQuestion>
    {
        public FormQuestionValidator(FormService formService, ConfigService configService, IStringLocalizer<FormQuestionValidator> localizer)
        {
            RuleFor(x => x.FormId).NotEmpty()
                .WithMessage(localizer["Must select form"].Value)
                .Must(x => formService.AnyFormId(x))
                .WithMessage(localizer["Form is not found"].Value);

            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.QuestionTypeCode).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must(x => configService.ValueInCategories(x, "QuestionType"))
                .WithMessage(localizer["Value is not in category"].Value);
        }
    }
}
