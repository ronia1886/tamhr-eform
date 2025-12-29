using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class FormQuestionDetailValidator : AbstractValidator<FormQuestionDetail>
    {
        public FormQuestionDetailValidator(FormService formService, ConfigService configService, IStringLocalizer<FormQuestionDetailValidator> localizer)
        {
            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.DescriptionType).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);

            RuleFor(x => x.OrderSequence).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);
        }
    }
}
