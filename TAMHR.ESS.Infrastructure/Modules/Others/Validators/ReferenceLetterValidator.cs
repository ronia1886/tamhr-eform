using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ReferenceLetterValidator : AbstractDocumentValidator<ReferenceLetterViewModel>
    {
        public ReferenceLetterValidator(ConfigService configService, IStringLocalizer<ReferenceLetterValidator> localizer)
        {
            RuleFor(x => x.Object.ReferenceLetterPurposeCode).NotEmpty()
                .WithMessage(localizer["Please select purpose"].Value)
                .Must(x => configService.ValueInCategories(x, "ReferenceLetterPurpose"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Object.FilePath).NotEmpty()
                .WithMessage(localizer["Please upload a file"].Value);

            RuleFor(x => x.Object.Remarks).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));
        }
    }
}
