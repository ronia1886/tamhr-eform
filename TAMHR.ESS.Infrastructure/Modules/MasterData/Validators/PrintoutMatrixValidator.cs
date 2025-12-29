using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PrintoutMatrixValidator : AbstractValidator<PrintoutMatrix>
    {
        public PrintoutMatrixValidator(ConfigService configService, IStringLocalizer<PrintoutMatrix> localizer)
        {
            RuleFor(x => x.FormId).NotEmpty();

            RuleFor(x => x.Approver).NotEmpty()
                .WithMessage(localizer["Approver must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.ApproverType).NotEmpty()
                .WithMessage(localizer["Approver type must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.ApproverLocation).NotEmpty()
                .WithMessage(localizer["Approver location must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));
        }
    }
}
