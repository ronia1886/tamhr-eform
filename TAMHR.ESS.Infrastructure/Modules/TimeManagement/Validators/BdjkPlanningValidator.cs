using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BdjkPlanningValidator : AbstractDocumentValidator<BdjkPlanningViewModel>
    {
        public BdjkPlanningValidator(ConfigService configService, IStringLocalizer<BdjkPlanningValidator> localizer)
        {
            var categories = configService.GetGeneralCategories("CategorySPKL");

            RuleFor(x => x.Object.Period).NotEmpty();
            RuleFor(x => x.Object.Details).Must(x => x.Count() > 0);
            RuleForEach(x => x.Object.Details).SetValidator(new ItemValidator(categories, localizer));
        }

        public class ItemValidator : AbstractValidator<BdjkRequestViewModel>
        {
            public ItemValidator(IEnumerable<GeneralCategory> categories, IStringLocalizer<BdjkPlanningValidator> localizer)
            {
                RuleFor(x => x.BdjkReason).NotEmpty()
                    .WithMessage(localizer["Reason cannot be empty"].Value)
                    .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                    .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));

                RuleFor(x => x.ActivityCode).NotEmpty()
                    .WithMessage("Please select activity")
                    .Must(x => categories.Any(y => y.Code == x))
                    .WithMessage(localizer["Value is not in category"].Value);
            }
        }
    }
}
