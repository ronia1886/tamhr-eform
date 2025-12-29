using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class NewsValidator : AbstractValidator<News>
    {
        public NewsValidator(IStringLocalizer<NewsValidator> localizer)
        {
            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.SlugUrl).NotEmpty()
                .WithMessage(localizer["Slug url must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.ShortDescription).NotEmpty()
                .WithMessage(localizer["Short description must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.BodyHtml).NotEmpty()
                .WithMessage(localizer["Body HTML must be filled"].Value);

            RuleFor(x => x.OrderIndex).NotEmpty()
                .WithMessage(localizer["Order index must be filled"].Value);

            RuleFor(x => x.ImageUrl).NotEmpty()
                .WithMessage(localizer["Image must be selected"].Value);
        }
    }
}
