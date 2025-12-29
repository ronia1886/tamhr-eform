using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AreaValidator : AbstractValidator<AreaTB>
    {
        public AreaValidator(ConfigService configService, IStringLocalizer<AreaTB> localizer)
        {
            RuleFor(x => x.NamaArea).NotEmpty()
                .WithMessage(localizer["Nama Area must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.DivisiCode).NotEmpty()
                .WithMessage(localizer["Division must be filled"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.VarcharMaxLength));

            RuleFor(x => x.Alamat).NotEmpty()
                .WithMessage(localizer["ALamat must be filled"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.VarcharMaxLength));
        }
    }
}
