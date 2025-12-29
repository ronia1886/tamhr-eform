using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class KlinikValidator : AbstractValidator<KlinikTB>
    {
        public KlinikValidator(ConfigService configService, IStringLocalizer<KlinikTB> localizer)
        {
            RuleFor(x => x.AreaIdKlinik).NotEmpty()
                .WithMessage(localizer["Area must be filled"].Value);
            //.MaximumLength(ApplicationConstants.MediumInputMaxLength)
            //.WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            //RuleFor(x => x.AreaId).NotEmpty()
            //    .WithMessage(localizer["Area name must be filled"].Value)
            //    .MaximumLength(ApplicationConstants.MediumInputMaxLength)
            //    .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.Klinik).NotEmpty()
                .WithMessage(localizer["UPKK must be filled"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.VarcharMaxLength));

            RuleFor(x => x.CategoryCode).NotEmpty()
                .WithMessage(localizer["Category Name must be filled"].Value)
                .MaximumLength(ApplicationConstants.RemarksMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.FromHours).NotEmpty()
                .WithMessage(localizer["FromHours must be filled"].Value);

            RuleFor(x => x.ToHours).NotEmpty()
                .WithMessage(localizer["ToHours must be filled"].Value);

            RuleFor(x => x.PIC).NotEmpty()
                .WithMessage(localizer["PIC must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

           
        }
    }
}
