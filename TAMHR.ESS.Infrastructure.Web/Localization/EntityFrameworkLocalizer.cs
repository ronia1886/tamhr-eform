using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;

namespace TAMHR.ESS.Infrastructure.Web.Localization
{
    public class EntityFrameworkLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, string> _dicts;

        public EntityFrameworkLocalizer(ConcurrentDictionary<string, string> dicts)
        {
            _dicts = dicts;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = string.Empty;
                _dicts.TryGetValue(CultureInfo.CurrentUICulture.Name + "|" + name, out value);

                return new LocalizedString(name, string.IsNullOrEmpty(value) ? name : value, resourceNotFound: string.IsNullOrEmpty(value));
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = string.Empty;
                _dicts.TryGetValue(CultureInfo.CurrentUICulture.Name + "|" + name, out value);
                var output = string.Format(value, arguments);

                return new LocalizedString(name, output, resourceNotFound: string.IsNullOrEmpty(value));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;

            return new EntityFrameworkLocalizer(_dicts);
        }
    }
}
