using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ComplaintRequestValidator : AbstractDocumentValidator<ComplaintRequestViewModel>
    {
        public ComplaintRequestValidator(ConfigService configService, IStringLocalizer<ComplaintRequestValidator> localizer)
        {
            RuleFor(x => x.Object.ComplaintCode).NotEmpty()
                .WithMessage(localizer["Please select complaint type"].Value)
                .Must(x => configService.ValueInCategories(x, "ComplaintType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Object.ComplaintSubCode).NotEmpty()
                .WithMessage(localizer["Please select complaint sub type"].Value)
                .Must((context, x) => configService.ValueInCategoryMappings(context.Object.ComplaintSubCode, context.Object.ComplaintCode, "ComplaintType"))
                .WithMessage(localizer["Value is not in category mapping"].Value);

            RuleFor(x => x.Object.ComplaintDetail).NotEmpty()
                .WithMessage(localizer["Complaint detail must be filled"].Value)
                .MaximumLength(ApplicationConstants.RemarksMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.RemarksMaxLength));

            RuleFor(x => x.Object.Expectation).NotEmpty()
                .WithMessage(localizer["Expectation must be filled"].Value)
                .MaximumLength(ApplicationConstants.RemarksMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.RemarksMaxLength));

            RuleFor(x => x.Object.FilePath).NotEmpty()
                .WithMessage(localizer["Please upload a file"].Value);
        }
    }
}
