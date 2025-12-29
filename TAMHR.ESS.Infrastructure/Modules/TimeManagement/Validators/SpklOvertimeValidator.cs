using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class SpklOvertimeValidator : AbstractDocumentValidator<SpklOvertimeViewModel>
    {
        public SpklOvertimeValidator(ConfigService configService, IStringLocalizer<SpklOvertimeValidator> localizer)
        {
            When(x => x.Object.OvertimeType.Equals("form") && x.Object.MustBeValidated, () => {
                //var minDate = new DateTime(DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year, DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1, 1);
                var minDate = DateTime.Now.Date;

                RuleFor(x => x.Object.OvertimeDate).NotEmpty()
                    .WithMessage(localizer["Overtime date must be filled"].Value)
                    .GreaterThanOrEqualTo(minDate)
                    .WithMessage(string.Format(localizer["Overtime date must be greater than or equal to {0:dd/MM/yyyy}"].Value, minDate));

                RuleFor(x => x.Object.OvertimeHourIn).NotEmpty()
                    .WithMessage(localizer["Overtime hour in must be filled"].Value)
                    .Must((data, val) => (data.Object.OvertimeDate == data.Object.OvertimeDateOut && data.Object.OvertimeHourOut > data.Object.OvertimeHourIn) || data.Object.OvertimeDateOut > data.Object.OvertimeDate)
                    .WithMessage(localizer["Overtime hour out must be greater than overtime hour in"].Value);

                RuleFor(x => x.Object.OvertimeDateOut).NotEmpty()
                    .WithMessage(localizer["Overtime date out must be filled"].Value)
                    .GreaterThanOrEqualTo(x => x.Object.OvertimeDate)
                    .WithMessage(string.Format(localizer["Overtime date out must be greater than or equal to overtime date"].Value));

                RuleFor(x => x.Object.OvertimeHourOut).NotEmpty()
                    .WithMessage(localizer["Overtime hour out must be filled"].Value);

                RuleFor(x => x.Object.Category).NotEmpty()
                    .WithMessage(localizer["Please select category"].Value)
                    .Must(x => configService.ValueInCategories(x, "CategorySPKL"))
                    .WithMessage(localizer["Value is not in category"].Value);

                RuleFor(x => x.Object.Reason).NotEmpty()
                    .WithMessage(localizer["Reason must be filled"].Value);
            });
        }
    }
}
