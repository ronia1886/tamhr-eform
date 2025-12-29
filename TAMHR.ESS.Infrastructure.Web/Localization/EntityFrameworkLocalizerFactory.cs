using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Modules.Core.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;

namespace TAMHR.ESS.Infrastructure.Web.Localization
{
    /// <summary>
    /// Entity framework localizer factory class (the instance of this class is registered as singleton).
    /// </summary>
    public class EntityFrameworkLocalizerFactory : IStringLocalizerFactory
    {
        #region Variables & Properties
        // Concurrent dictionary for multithreading environment.
        private readonly ConcurrentDictionary<string, string> _dicts;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="services">This <see cref="IServiceProvider"/> concrete object.</param>
        public EntityFrameworkLocalizerFactory(PathProvider pathProvider, IServiceProvider services)
        {
            var fileName = Path.GetFileName("culture-new.all.json");
            var cultureJsonFile = Path.Combine(pathProvider.ContentPath("cultures"), fileName);

            if (File.Exists(cultureJsonFile))
            {
                using (var fileReader = new StreamReader(cultureJsonFile))
                {
                    var content = fileReader.ReadToEnd();
                    _dicts = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(content);
                }
            }
            else
            {
                using (var scope = services.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<LanguageService>();
                    _dicts = new ConcurrentDictionary<string, string>(service.GetLanguages());
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update language cache by key.
        /// </summary>
        /// <param name="key">This key (combination of culture code and translate key).</param>
        /// <param name="value">This translate value.</param>
        public void Update(string key, string value)
        {
            // If given key exist in dictionary then update the dictionary.
            if (_dicts.ContainsKey(key))
            {
                // Update the dictionary by key.
                _dicts[key] = value;
            }
            // Else create new dictionary by given key and value.
            else
            {
                // Create new dictionary by given key and value.
                _dicts.TryAdd(key, value);
            }
        }

        /// <summary>
        /// Remove language cache by key.
        /// </summary>
        /// <param name="key">This key (combination of culture code and translate key).</param>
        public void Remove(string key)
        {
            // If given key exist in dictionary then remove from the dictionary.
            if (_dicts.ContainsKey(key))
            {
                // Create temporary value input string.
                var value = string.Empty;

                // Remove dictionary by given key and save the result into value variable.
                _dicts.TryRemove(key, out value);
            }
        }
        #endregion

        #region Implementation of IStringLocalizerFactory
        /// <summary>
        /// Create localizer resources by resource type.
        /// </summary>
        /// <param name="resourceSource">This resource type.</param>
        /// <returns>This <see cref="IStringLocalizer"/> concrete object.</returns>
        public IStringLocalizer Create(Type resourceSource)
        {
            // Return entity framework localizer object.
            return new EntityFrameworkLocalizer(_dicts);
        }

        /// <summary>
        /// Create localizer resources by base name and location.
        /// </summary>
        /// <param name="baseName">This resource base name.</param>
        /// <param name="location">THis resource location.</param>
        /// <returns>This <see cref="IStringLocalizer"/> concrete object.</returns>
        public IStringLocalizer Create(string baseName, string location)
        {
            // Return entity framework localizer object.
            return new EntityFrameworkLocalizer(_dicts);
        }
        #endregion
    }
}
