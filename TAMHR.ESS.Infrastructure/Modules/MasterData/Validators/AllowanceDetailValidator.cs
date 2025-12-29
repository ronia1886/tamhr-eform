using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AllowanceDetailValidator : AbstractValidator<AllowanceDetail>
    {
        public AllowanceDetailValidator(ConfigService configService, IStringLocalizer<AllowanceDetail> localizer)
        {
            RuleFor(x => x.Type).NotEmpty()
                .WithMessage(localizer["Type must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.ClassFrom).NotEmpty()
                .WithMessage(localizer["Class from must be filled"].Value)
                .GreaterThan(0)
                .WithMessage(localizer["Value must be greater than 0"].Value);

            RuleFor(x => x.ClassTo).NotEmpty()
                .WithMessage(localizer["Class to must be filled"].Value)
                .GreaterThan(0)
                .WithMessage(localizer["Value must be greater than 0"].Value);

            RuleFor(x => x.Ammount).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value);

            RuleFor(x => x.StartDate).NotEmpty()
                .WithMessage(localizer["Start date must be filled"].Value);

            RuleFor(x => x.EndDate).NotEmpty()
                .WithMessage(localizer["End date must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);
        }
    }
}
