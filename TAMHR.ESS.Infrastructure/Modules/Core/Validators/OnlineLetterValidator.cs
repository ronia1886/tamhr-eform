using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class OnlineLetterValidator : AbstractValidator<OnlineLetter>
    {
        public OnlineLetterValidator(ConfigService configService, IStringLocalizer<OnlineLetter> localizer)
        {
            RuleFor(x => x.LetterDate).NotEmpty();
            RuleFor(x => x.Department).NotEmpty();
            RuleFor(x => x.LetterTypeCode).NotEmpty();
            RuleFor(x => x.PicTarget).NotEmpty();
            RuleFor(x => x.CompanyTarget).NotEmpty();
        }
    }
}
