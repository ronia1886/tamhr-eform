using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class KategoriPenyakitValidator : AbstractValidator<Kategori_Penyakit>
    {
        public KategoriPenyakitValidator(ConfigService configService, IStringLocalizer<Kategori_Penyakit> localizer)
        {
            RuleFor(x => x.IdTingkatSakit.ToString()).NotEmpty()
                .WithMessage(localizer["Tingkat sakit must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallVarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallVarcharMaxLength));

            RuleFor(x => x.KategoriPenyakit).NotEmpty()
                .WithMessage(localizer["Kategori Penyakit must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));

            RuleFor(x => x.Penyakit).NotEmpty()
                .WithMessage(localizer["Penyakit must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
    }
}
