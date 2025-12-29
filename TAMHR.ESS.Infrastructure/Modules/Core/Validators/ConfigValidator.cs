using System.Globalization;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common.Utility;
using FluentValidation;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ConfigValidator : AbstractValidator<Config>
    {
        public ConfigValidator(ConfigService configService, IStringLocalizer<ConfigValidator> localizer)
        {
            RuleFor(x => x.ModuleCode).NotEmpty()
                .WithMessage(localizer["Please select module"].Value)
                .Must(x => configService.ValueInCategories(x, "Module"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.ConfigKey).NotEmpty()
                .WithMessage(localizer["Config key must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.ConfigText).NotEmpty()
                .WithMessage(localizer["Config text must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));

            // Define config value validation
            // 1. Not empty string.
            // 2. With maximum 8000 character length.
            RuleFor(x => x.ConfigValue).NotEmpty()
                .WithMessage(localizer["Cofig Value must be filled"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.VarcharMaxLength))
                .Must((data, x) =>
                {
                    if (data.DataTypeCode == "bool")
                    {
                        return ObjectHelper.IsIn(x, new[] { "True", "False" });
                    }
                    else if (data.DataTypeCode == "int")
                    {
                        return decimal.TryParse(x, NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out decimal output);
                    }
                    else if (data.DataTypeCode == "date")
                    {
                        return DateTime.TryParseExact(x, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime output);
                    }

                    return true;
                })
                .WithMessage(string.Format(localizer["Value with data type didnt match"].Value, ApplicationConstants.VarcharMaxLength));

            RuleFor(x => x.DataTypeCode).NotEmpty()
                .WithMessage(localizer["Please select data type"].Value)
                .Must(x => configService.ValueInCategories(x, "DataType"))
                .WithMessage(localizer["Value is not in category"].Value);
        }
    }
}
