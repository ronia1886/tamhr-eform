using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ProxyTimeFormValidator : AbstractValidator<ProxyTime>
    {
        public ProxyTimeFormValidator(ConfigService configService, IStringLocalizer<ProxyTimeFormValidator> localizer)
        {
            When(x => x.Id == Guid.Empty, () =>
            {
                RuleFor(x => x.GeoLocation).NotEmpty()
                    .WithMessage(localizer["Geo location must be filled"].Value);

                RuleFor(x => x.Latitude).NotEmpty()
                    .WithMessage(localizer["Latitude must be filled"].Value);
                
                RuleFor(x => x.Longitude).NotEmpty()
                    .WithMessage(localizer["Longitude must be filled"].Value);

                RuleFor(x => x.WorkingTypeCode).NotEmpty()
                   .WithMessage(localizer["Please select working type"].Value)
                   .Must(x => configService.ValueInCategories(x, "WorkingType"))
                   .WithMessage(localizer["Value is not in category"].Value);
            });
        }
    }
}
