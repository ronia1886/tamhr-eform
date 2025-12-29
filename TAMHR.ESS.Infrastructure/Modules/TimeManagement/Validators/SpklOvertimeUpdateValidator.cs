using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class SpklOvertimeUpdateCollectionValidator : AbstractValidator<SpklOvertimeUpdateViewModel[]>
    {
        public SpklOvertimeUpdateCollectionValidator(ConfigService configService, IStringLocalizer<SpklOvertimeUpdateCollectionValidator> localizer)
        {
            //RuleFor(x => x).SetCollectionValidator(new SpklOvertimeUpdateValidator(configService, localizer));
            RuleForEach(collection => collection).SetValidator(new SpklOvertimeUpdateValidator(configService, localizer));
        }
    }

    public class SpklOvertimeUpdateValidator : AbstractValidator<SpklOvertimeUpdateViewModel>
    {
        public SpklOvertimeUpdateValidator(ConfigService configService, IStringLocalizer<SpklOvertimeUpdateCollectionValidator> localizer)
        {
            RuleFor(x => x.OvertimeIn).NotEmpty()
                .WithMessage(localizer["Overtime date in must be filled"].Value);

            RuleFor(x => x.OvertimeOut).NotEmpty()
                .WithMessage(localizer["Overtime date out must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.OvertimeIn)
                .WithMessage(localizer["Overtime date out must be greater than or equal to overtime date in"].Value);

            RuleFor(x => x.OvertimeInAdjust).NotEmpty()
                .WithMessage(localizer["Overtime date in adjust must be filled"].Value)
                .Must((obj, x) => obj.OvertimeIn.Date == x.Date)
                .WithMessage(localizer["Overtime date in adjust must be qual to overtime date in"].Value);

            RuleFor(x => x.OvertimeOutAdjust).NotEmpty()
                .WithMessage(localizer["Overtime date out adjust must be filled"].Value)
                .Must((obj, x) => obj.OvertimeOut.Date == x.Date)
                .WithMessage(localizer["Overtime date out adjust must be qual to overtime date out"].Value)
                .GreaterThanOrEqualTo(x => x.OvertimeInAdjust)
                .WithMessage(localizer["Overtime date out adjust must be greater than or equal to overtime date in adjust"].Value);

            RuleFor(x => x.DurationAdjust).NotEmpty()
                .WithMessage(localizer["Duration adjust must be filled"].Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Duration adjust must be greater than or equal to 0"].Value);
        }
    }
}
