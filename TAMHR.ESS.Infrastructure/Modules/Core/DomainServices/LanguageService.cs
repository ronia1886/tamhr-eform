using Agit.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.Modules.Core.DomainServices
{
    public class LanguageService : GenericDomainServiceBase<Language>
    {

        public DbSet<Language> Language { get; set; }

        #region Properties Member
        /// <summary>
        /// List of fields to update.
        /// </summary>
        protected override string[] Properties => new[] { "CultureCode", "TranslateKey", "TranslateValue" };
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="unitOfWork">Unit of work object.</param>
        public LanguageService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of languages from cache.
        /// </summary>
        /// <returns>This list of <see cref="Language"/> object in dictionary format.</returns>
        public Dictionary<string, string> GetLanguages()
        {
            var languages = Repository.Fetch()
                .AsNoTracking().FromCache("language")
                .GroupBy(x => x.CultureCode + "|" + x.TranslateKey)
                .Select(x => new { x.Key, Value = x.First().TranslateValue });

            return languages.ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<string, string> GetLanguages(string cultureCode)
        {
            var languages = Repository.Fetch().AsNoTracking().FromCache("language")
                .Where(x => x.CultureCode == cultureCode)
                .GroupBy(x => x.TranslateKey)
                .Select(x => new { x.Key, Value = x.First().TranslateValue });

            return languages.ToDictionary(x => x.Key, x => x.Value);
        }
        #endregion
    }
}
