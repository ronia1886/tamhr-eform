using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ApprovalMatrixValidator : AbstractValidator<ApprovalMatrix>
    {
        public ApprovalMatrixValidator(ConfigService configService, IStringLocalizer<ApprovalMatrixValidator> localizer)
        {
            RuleFor(x => x.FormId).NotEmpty();

            RuleFor(x => x.Approver).NotEmpty()
                .WithMessage(localizer["Approver must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.ApproverType).NotEmpty()
                .WithMessage(localizer["Approver type must be filled"].Value);

            RuleFor(x => x.ApproverLevel).NotEmpty()
                .WithMessage(localizer["Approver level must be filled"].Value)
                .GreaterThan(0)
                .WithMessage(localizer["Approver level must be greater than 0"].Value);

            RuleFor(x => x.StartDate).NotEmpty()
                .WithMessage(localizer["Start date must be filled"].Value);

            RuleFor(x => x.EndDate).NotEmpty()
                .WithMessage(localizer["End date must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);
        }
    }
}
