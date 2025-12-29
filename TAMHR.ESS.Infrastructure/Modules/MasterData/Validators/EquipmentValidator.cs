using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EquipmentValidator : AbstractValidator<Equipment>
    {
        public EquipmentValidator(ConfigService configService, IStringLocalizer<Equipment> localizer)
        {
            RuleFor(x => x.DivisiCodeEquip).NotEmpty()
                .WithMessage(localizer["Divisi must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.AreaIdEquip).NotEmpty()
                .WithMessage(localizer["Area name must be filled"].Value);

            RuleFor(x => x.EquipmentName).NotEmpty()
                .WithMessage(localizer["Equipment Name must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallVarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallVarcharMaxLength));

            //RuleFor(x => x.Quantity)
            //.NotEmpty()
            //.WithMessage(localizer["Quantity must be filled"].Value)
            //.GreaterThanOrEqualTo(0)
            //.WithMessage(localizer["Value must be 0 or greater"].Value);

            RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);


        }
    }
}
