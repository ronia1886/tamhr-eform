using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EventCalendarValidator : AbstractValidator<EventsCalendar>
    {
        public EventCalendarValidator(ConfigService configService, IStringLocalizer<EventCalendarValidator> localizer)
        {
            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.EventTypeCode).NotEmpty()
                .WithMessage(localizer["Please select event type"].Value)
                .Must(x => configService.ValueInCategories(x, "EventType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.StartDate).NotEmpty()
                .WithMessage(localizer["Start date must be filled"].Value);

            RuleFor(x => x.EndDate).NotEmpty()
                .WithMessage(localizer["End date must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
    }
}
