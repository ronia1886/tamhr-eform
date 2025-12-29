using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class LostAndReturnValidator : AbstractDocumentValidator<LostAndReturnViewModel>
    {
        public LostAndReturnValidator(ConfigService configService, IStringLocalizer<LostAndReturnValidator> localizer)
        {
            RuleFor(x => x.Object.DocumentCategoryCode).NotEmpty()
                .WithMessage(localizer["Please select document category"].Value)
                .Must(x => configService.ValueInCategories(x, "LoadAndReturnType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Object.CategoryCode).NotEmpty()
                .WithMessage(localizer["Please select category"].Value)
                .Must(x => configService.ValueInCategories(x, "LostAndReturnCategory"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Object.DamagedRemarks).NotEmpty()
                .WithMessage(localizer["Damaged remarks be filled"].Value)
                .MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength))
                .When(x => x.Object.CategoryCode == "return");

            RuleFor(x => x.Object.LostDate).NotNull()
                .WithMessage(localizer["Lost date must be filled"].Value)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage(localizer["Lost date must be lower than or equal to now"].Value)
                .When(x => x.Object.CategoryCode == "lost");

            RuleFor(x => x.Object.Location).NotEmpty()
                .WithMessage(localizer["Location must be filled"].Value)
                .When(x => x.Object.CategoryCode == "lost");

            RuleFor(x => x.Object.FilePath).NotEmpty()
                .WithMessage(localizer["Please upload a file"].Value);

            RuleFor(x => x.Object.Remarks).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));
        }
    }
}
