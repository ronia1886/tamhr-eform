using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle configuration and general category.
    /// </summary>
    public class ConfigService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Config repository object.
        /// </summary>
        protected IRepository<Config> ConfigRepository => UnitOfWork.GetRepository<Config>();

        /// <summary>
        /// Config classification repository object.
        /// </summary>
        protected IRepository<ConfigClassification> ConfigClassificationRepository => UnitOfWork.GetRepository<ConfigClassification>();

        /// <summary>
        /// General category repository object.
        /// </summary>
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();

        /// <summary>
        /// General category mapping repository object.
        /// </summary>
        protected IRepository<GeneralCategoryMapping> GeneralCategoryMappingRepository => UnitOfWork.GetRepository<GeneralCategoryMapping>();

        /// <summary>
        /// General category mapping readonly repository object.
        /// </summary>
        protected IReadonlyRepository<GeneralCategoryMappingView> GeneralCategoryMappingReadonlyRepository => UnitOfWork.GetRepository<GeneralCategoryMappingView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for config entity.
        /// </summary>
        private readonly string[] _configProperties = new[] { "ModuleCode", "ConfigKey", "ConfigText", "ConfigValue", "DataTypeCode" };

        /// <summary>
        /// Field that hold properties that can be updated for general category entity.
        /// </summary>
        private readonly string[] _generalCategoryProperties = new[] { "Code", "Category", "Name", "Description", "OrderSequence" };

        /// <summary>
        /// Field that hold properties that can be updated for readonly general category entity.
        /// </summary>
        private readonly string[] _readonlyGeneralCategoryProperties = new[] { "Name", "Description" };

        /// <summary>
        /// Field that hold cache tags for config.
        /// </summary>
        private readonly string[] _configTags = new[] { "config" };

        /// <summary>
        /// Field that hold cache tags for general category.
        /// </summary>
        private readonly string[] _generalCategoryTags = new[] { "general_category" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ConfigService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Config Area
        /// <summary>
        /// Get timeout configuration.
        /// </summary>
        /// <returns>This <see cref="TimeoutViewModel"/> object.</returns>
        public TimeoutViewModel GetTimeoutConfig()
        {
            // Get list of configs from cache.
            var configs = GetConfigs(true);

            // Create new timeout view model from given list of configs.
            return TimeoutViewModel.CreateFrom(configs);
        }

        /// <summary>
        /// Resolve avatar url.
        /// </summary>
        /// <param name="arguments">This string arguments.</param>
        /// <returns>This avatar url.</returns>
        public string ResolveAvatar(string arguments)
        {
            // Get avatar url from configuration.
            var config = GetConfig("Avatar.Url", true)?.ConfigValue;

            // Return the formatted avatar url.
            return string.Format(config, arguments);
        }

        /// <summary>
        /// Get list of configs.
        /// </summary>
        /// <param name="cache">This cache flag (if true then get from cache).</param>
        /// <returns>This list of <see cref="Config"/> objects.</returns>
        public IEnumerable<Config> GetConfigs(bool cache = false)
        {
            // Get config query object without object tracking.
            var configs = ConfigRepository.Fetch()
                .AsNoTracking();
            
            // If cache was enabled then get from cache.
            return cache
                ? configs.FromCache(_configTags)
                : configs.ToList();
        }

        /// <summary>
        /// Get config by id.
        /// </summary>
        /// <param name="id">This config id.</param>
        /// <returns>This <see cref="Config"/> object.</returns>
        public Config GetConfig(Guid id)
        {
            // Get config by id without object tracking and return default if empty.
            return ConfigRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get config by key.
        /// </summary>
        /// <param name="configKey">This config key.</param>
        /// <param name="cache">This cache flag (if true then get from cache).</param>
        /// <returns>This <see cref="Config"/> object.</returns>
        public Config GetConfig(string configKey, bool cache = false)
        {
            // If cache was enabled then get from cache.
            return cache
                ? GetConfigs(true).FirstOrDefault(x => x.ConfigKey == configKey)
                : ConfigRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.ConfigKey == configKey);
        }

        /// <summary>
        /// Get config value by key.
        /// </summary>
        /// <typeparam name="T">This value type.</typeparam>
        /// <param name="configKey">This config key.</param>
        /// <param name="defaultValue">This default value if not present.</param>
        /// <param name="cache">Determine whether the data should be get from cache or not.</param>
        /// <returns>This config value.</returns>
        public T GetConfigValue<T>(string configKey, T defaultValue, bool cache = false, IFormatProvider formatProvider = null)
        {
            // Get config by key.
            var config = GetConfig(configKey, cache);

            // If not exist or empty then return default value.
            if (config == null || string.IsNullOrEmpty(config.ConfigValue)) return defaultValue;

            if (formatProvider != null) return (T)Convert.ChangeType(config?.ConfigValue, typeof(T), formatProvider);

            // Return the config value based on given type.
            return (T)Convert.ChangeType(config?.ConfigValue, typeof(T));
        }

        /// <summary>
        /// Get config value by key.
        /// </summary>
        /// <typeparam name="T">This value type.</typeparam>
        /// <param name="configKey">This config key.</param>
        /// <param name="cache">Determine whether the data should be get from cache or not.</param>
        /// <returns>This config value.</returns>
        public T GetConfigValue<T>(string configKey, bool cache = false)
        {
            // Get config by key.
            var config = GetConfig(configKey, cache);

            // Return the config value based on given type.
            return (T)Convert.ChangeType(config?.ConfigValue, typeof(T));
        }

        /// <summary>
        /// Update or insert config.
        /// </summary>
        /// <param name="config">This <see cref="Config"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertConfig(Config config)
        {
            // Update or insert config with specified list of properties to update.
            ConfigRepository.Upsert<Guid>(config, _configProperties);

            // Clear config cache.
            QueryCacheManager.ExpireTag(_configTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete config by id and its dependencies if any
        /// </summary>
        /// <param name="id">Config Id</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeleteConfig(Guid id)
        {
            // Get config by id.
            var config = ConfigRepository.FindById(id);

            // Update the row status value to false.
            config.RowStatus = false;

            // Clear config cache.
            QueryCacheManager.ExpireTag(_configTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete config by id and its dependencies if any
        /// </summary>
        /// <param name="id">Config Id</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteConfig(Guid id)
        {
            // Mark config object as deleted.
            ConfigRepository.DeleteById(id);

            // Clear config cache.
            QueryCacheManager.ExpireTag(_configTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region General Category Area
        /// <summary>
        /// Get list of general categories.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of <see cref="GeneralCategory"/> objects.</returns>
        public IEnumerable<GeneralCategory> GetGeneralCategories(bool cache = false)
        {
            // Get general category query objects without object tracking.
            var generalCategories = GeneralCategoryRepository.Fetch()
                .AsNoTracking();

            // If cache was enabled then get from cache.
            return cache
                ? generalCategories.FromCache(_generalCategoryTags)
                : generalCategories.ToList();
        }
       
        /// <summary>
        /// Get general category query objects.
        /// </summary>
        /// <returns>This <see cref="GeneralCategory"/> query objects.</returns>
        public IQueryable<GeneralCategory> GetGeneralCategoriesQuery()
        {
            // Get general category query objects without object tracking.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get general category query objects by category.
        /// </summary>
        /// <param name="category">This category.</param>
        /// <returns>This <see cref="GeneralCategory"/> query objects.</returns>
        public IQueryable<GeneralCategory> GetGeneralCategoriesQuery(string category)
        {
            // Get general category query objects by category without object tracking ordered by name.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category)
                .OrderBy(x => x.Name);
        }
        public IQueryable<GeneralCategory> GetGeneralCategoriesQuerySortedSequence(string category)
        {
            // Get general category query objects by category without object tracking ordered by name.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category)
                .OrderBy(x => x.OrderSequence);
        }

        public IQueryable<GeneralCategory> GetGeneralCategories()
        {
            // Get general category query objects by category without object tracking ordered by name.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Get list of general categories by category.
        /// </summary>
        /// <param name="category">This category.</param>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="GeneralCategory"/> objects.</returns>
        public IEnumerable<GeneralCategory> GetGeneralCategories(string category, bool cache = false)
        {
            // Get general category query objects by category without object tracking.
            var generalCategories = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category)
                .OrderBy(x => x.Name);

            // If cache was enabled then get from cache.
            return cache
                ? generalCategories.FromCache($"general_category_{category.ToLower()}").ToList()
                : generalCategories.ToList();
        }

        /// <summary>
        /// Get list of custom general categories by category.
        /// </summary>
        /// <param name="category">This category.</param>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of custom general category objects.</returns>
        public IEnumerable<object> GetCustomGeneralCategories(string category, bool cache = false)
        {
            // Get general category query objects by category without object tracking.
            var generalCategories = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category)
                .OrderBy(x => x.Name)
                .Select(x => new { NameDesc = x.Name + " - " + x.Description });

            // If cache was enabled then get from cache.
            return cache
                ? generalCategories.FromCache($"general_category_{category.ToLower()}").ToList()
                : generalCategories.ToList();
        }

        /// <summary>
        /// Get list of custom general categories by category and map.
        /// </summary>
        /// <param name="category">This category.</param>
        /// <param name="categorymapping">This mapping category.</param>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of custom general category objects.</returns>
        public IEnumerable<object> GetCustomShiftGeneralCategories(string category, string categorymapping, bool cache = false)
        {
            // Get general category query objects by category without object tracking.
            var data = from gc in GeneralCategoryRepository.Fetch().AsNoTracking().Where(x => x.Category == category)
                       join gcm in GeneralCategoryMappingRepository.Fetch().AsNoTracking().Where(x => x.ParentGeneralCategoryCode == categorymapping)
                       on gc.Code equals gcm.GeneralCategoryCode
                       orderby gc.OrderSequence ascending
                       select new { NameDesc = gc.Name + " - " + gc.Description };

            // If cache was enabled then get from cache.
            return cache
                ? data.FromCache($"general_category_{category.ToLower()}").ToList()
                : data.ToList();
        }

        /// <summary>
        /// Get general category by id.
        /// </summary>
        /// <param name="id">This general category id.</param>
        /// <returns>This <see cref="GeneralCategory"/> object.</returns>
        public GeneralCategory GetGeneralCategory(Guid id)
        {
            // Get general category by id without object tracking and return default if empty.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        
        /// <summary>
        /// Get general category by code.
        /// </summary>
        /// <param name="code">This general category code.</param>
        /// <returns>This <see cref="GeneralCategory"/> object.</returns>
        public GeneralCategory GetGeneralCategory(string code)
        {
            // Get general category by code without object tracking and return default if empty.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Code == code)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Check whether the general category value with specified category is exist in the list or not.
        /// </summary>
        /// <param name="value">This general category value.</param>
        /// <param name="category">This category.</param>
        /// <returns>True if exist, false otherwise.</returns>
        public bool ValueInCategories(string value, string category)
        {
            // Determine whether given general category value with specified category is exist or not.
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.Category == category && x.Code == value);
        }

        /// <summary>
        /// Check whether the value is in general category mapping or not.
        /// </summary>
        /// <param name="value">This general category value.</param>
        /// <param name="parentValue">This general category parent value.</param>
        /// <param name="category">This category.</param>
        /// <returns>True if exist, false otherwise.</returns>
        public bool ValueInCategoryMappings(string value, string parentValue, string category)
        {
            // Determine whether given parent value with specified category is exist or not.
            var inCategory = ValueInCategories(parentValue, category);

            // Determine whether given general category was map to parent value or not.
            return inCategory && GeneralCategoryMappingRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.ParentGeneralCategoryCode == parentValue && x.GeneralCategoryCode == value);
        }

        /// <summary>
        /// Update or insert general category.
        /// </summary>
        /// <param name="generlCategory">This <see cref="GeneralCategory"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertGeneralCategory(GeneralCategory generalCategory)
        {
            // Get and set list of properties to update.
            var propertiesToUpdate = generalCategory.Readonly
                ? _readonlyGeneralCategoryProperties
                : _generalCategoryProperties;

            // Update or insert general category with specified list of properties to update.
            GeneralCategoryRepository.Upsert<Guid>(generalCategory, propertiesToUpdate);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Update or insert general category mapping.
        /// </summary>
        /// <param name="generlCategoryMapping">This <see cref="GeneralCategoryMapping"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertGeneralCategoryMapping(GeneralCategoryMapping generalCategoryMapping)
        {
            // Update or insert general category mapping with specified list of properties to update.
            GeneralCategoryMappingRepository.Upsert<Guid>(generalCategoryMapping);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete general category by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This general category id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeleteGeneralCategory(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get general category by id.
                var generalCategory = GeneralCategoryRepository.FindById(id);

                // Throw an exception if general category object is readonly.
                Assert.ThrowIf(generalCategory.Readonly, $"Cannot delete general category with code '{generalCategory.Code}' because its readonly");

                // Update the row status value to false.
                generalCategory.RowStatus = false;

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Delete general category by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This general category id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteGeneralCategory(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            UnitOfWork.Transact(() =>
            {
                // Get general category by id.
                var generalCategory = GeneralCategoryRepository.FindById(id);

                // Throw an exception if general category object is readonly.
                Assert.ThrowIf(generalCategory.Readonly, $"Cannot delete general category with code '{generalCategory.Code}' because its readonly");

                // Mark given general category object as deleted.
                GeneralCategoryRepository.Delete(generalCategory);

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Delete general category mapping by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This general category mapping id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteGeneralCategoryMapping(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            UnitOfWork.Transact(() =>
            {
                // Get general category mapping by id.
                var generalCategoryMapping = GeneralCategoryMappingRepository.FindById(id);

                // Mark given general category mapping object as deleted.
                GeneralCategoryMappingRepository.Delete(generalCategoryMapping);

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }
        #endregion

        #region General Category Mapping
        /// <summary>
        /// Get list of category mapping objects by code.
        /// </summary>
        /// <param name="code">This general category code.</param>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="GeneralCategoryMappingView"/> objects.</returns>
        public IEnumerable<GeneralCategoryMappingView> GetGeneralCategoryMappingQuery(string code)
        {
            // Get list of category mapping objects by code without object tracking ordered by name.
            return GeneralCategoryMappingReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(code) || x.Code == code)
                .OrderBy(x => x.Name);
        }
        public IEnumerable<GeneralCategoryMappingView> GetGeneralCategoryMapping()
        {
            // Get list of category mapping objects by code without object tracking ordered by name.
            return GeneralCategoryMappingReadonlyRepository.Fetch()
                .AsNoTracking()
                .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Get list of general categories by category mapping.
        /// </summary>
        /// <param name="parentCode">This general category parent code.</param>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of <see cref="GeneralCategoryMappingView"/> objects.</returns>
        public IEnumerable<GeneralCategoryMappingView> GetGeneralCategoryMapping(string parentCode, bool cache = false)
        {
            // Get category mapping query objects by parent code without object tracking.
            var query = GeneralCategoryMappingReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ParentGeneralCategoryCode == parentCode)
                .OrderBy(x => x.Name);

            // If cache was enabled then get from cache.
            return cache
                ? query.FromCache($"general_category_mapping_{parentCode.ToLower()}").ToList()
                : query.ToList();
        }
        #endregion

        #region Config Classification
        /// <summary>
        /// Get list of config classifications by category.
        /// </summary>
        /// <param name="category">This category.</param>
        /// <returns>This list of <see cref="ConfigClassification"/> objects.</returns>
        public IEnumerable<ConfigClassification> GetConfigClassifications(string category)
        {
            // Get list of config classifications by category without object tracking.
            return ConfigClassificationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == category);
        } 
        #endregion

        #region Period Area
        /// <summary>
        /// Get list of periods.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of <see cref="Period"/> objects.</returns>
        public IEnumerable<Period> GetPeriodYears()
        {
            // Get and set current period.
            var period = DateTime.Now.Year;
            
            // Get year threshold config object.
            var yearThresholdConfig = ConfigRepository.Find(x => x.ConfigText == "YearTreshold")
                .FirstOrDefault();

            // Get the integer value.
            var count = Convert.ToInt32(yearThresholdConfig.ConfigValue);

            // Create new list of periods.
            var periods = new List<Period>();

            // Enumerate from threshold.
            for (int i = count; i >= 0; i--)
            {
                // Create new period object and add it into the list.
                periods.Add(new Period
                {
                    // Get and set period.
                    Years = period - i,
                    // Get and set the counter.
                    id = i
                });
            }

            // Return the list.
            return periods;
        }

        /// <summary>
        /// Get list of periods of shift.
        /// </summary>
        /// <returns>This list of <see cref="Period"/> objects.</returns>
        public IEnumerable<Period> GetPeriodYearsShift()
        {
            // Determine whether current month is greater than the middle of the month in year.
            var greaterThanMiddle = DateTime.Now >= new DateTime(DateTime.Now.Year, 7, 1);
            
            // Get and set current period.
            var period = DateTime.Now.Year;

            // Create new list of periods.
            var periods = new List<Period>();

            // If current month was greater than the middle.
            if (!greaterThanMiddle)
            {
                // Enumerate two times.
                for (int i = 0; i <= 1; i++)
                {
                    // Create new period object and add it into the list.
                    periods.Add(new Period
                    {
                        // Get and set period.
                        Years = period - i,
                        // Get and set the counter.
                        id = i
                    });
                }

                // Get and set the sorted list by years.
                periods = periods.OrderBy(x => x.Years)
                    .ToList();
            }
            else
            {
                // Enumerate three times.
                for (int i = 0; i <= 2; i++)
                {
                    // Create new period object and add it into the list.
                    periods.Add(new Period
                    {
                        // Get and set period.
                        Years = period + i,
                        // Get and set the counter.
                        id = i
                    });
                }
            }

            // Return the list.
            return periods;
        }

        /// <summary>
        /// Get list of periods in one year.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="Period"/> objects.</returns>
        public IEnumerable<Period> GetPeriodMonth()
        {
            // Get list of month names from current culture.
            var names = DateTimeFormatInfo.CurrentInfo.MonthNames;

            // Create new list of periods.
            var periods = new List<Period>();

            // Iterate through list of month names.
            for (int i = 0; i < names.Length; i++)
            {
                // Create new period object and add it into the list.
                periods.Add(new Period
                {
                    // Get and set month name.
                    Month = names[i],
                    // Get and set month number.
                    id = i + 1
                });
            }

            // Return the list.
            return periods;
        }
        #endregion

        #region Abnormality Area

        protected class AbnormalityTemp
        {
            public string NoReg { get; set; }
            public DateTime WorkingDate { get; set; }
            public Guid Id { get; set; }
        }

        public void ChangedAbnormalityDate()
        {
            DateTime abnormalityStartDate = DateTime.Parse(GetConfig("Abnormality.StartDate").ConfigValue);
            DateTime abnormalityEndDate = DateTime.Parse(GetConfig("Abnormality.EndDate").ConfigValue);

            IRepository<DocumentApproval> DocumentApprovalRepository = UnitOfWork.GetRepository<DocumentApproval>();
            IRepository<DocumentRequestDetail> DocumentRequestDetailRepository = UnitOfWork.GetRepository<DocumentRequestDetail>();

            #region Abnormality Absence
                IRepository<AbnormalityAbsence> AbnormalityAbsenceRepository = UnitOfWork.GetRepository<AbnormalityAbsence>();

                List<AbnormalityTemp> listAbnormalityAbsenceDate = AbnormalityAbsenceRepository.Fetch().Where(x => (x.WorkingDate < abnormalityStartDate || x.WorkingDate > abnormalityEndDate) && x.AbnormalityStatus == "Progress").Select(x => new AbnormalityTemp(){ WorkingDate = x.WorkingDate, NoReg = x.NoReg, Id = x.Id }).ToList();
                var listAbnormalityAbsenceDocumentApprovalId = AbnormalityAbsenceRepository.Fetch().Where(x => (x.WorkingDate < abnormalityStartDate || x.WorkingDate > abnormalityEndDate) && x.AbnormalityStatus == "Progress").Select(x => x.DocumentApprovalId);

                if (listAbnormalityAbsenceDate.Count > 0)
                {
                    foreach (Guid abnormalityAbsenceDocumentApprovalId in listAbnormalityAbsenceDocumentApprovalId)
                    {
                        DocumentRequestDetail tempDocumentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == abnormalityAbsenceDocumentApprovalId).FirstOrDefault();

                        AbnormalityAbsenceViewModel tempAbnormalityAbsenceViewModel = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(tempDocumentRequestDetail.ObjectValue);

                        var tempDetailsList = tempAbnormalityAbsenceViewModel.Details.ToList();

                        //DELETE ABNORMALITY ABSENCE ROW
                        foreach (AbnormalityAbsenceView abnormalityAbsenceView in tempAbnormalityAbsenceViewModel.Details)
                        {
                            AbnormalityAbsence tempAbnormalityAbsence = AbnormalityAbsenceRepository.FindById(abnormalityAbsenceView.AbnormalityAbsenceId);
                            if (listAbnormalityAbsenceDate.Where(x => x.Id == tempAbnormalityAbsence.Id).Count() > 0)
                            {
                                tempAbnormalityAbsence.AbnormalityStatus = "Locked";

                                AbnormalityAbsenceRepository.Upsert<Guid>(tempAbnormalityAbsence, new[] { "AbnormalityStatus" });

                                tempDetailsList.Remove(abnormalityAbsenceView);
                            }
                        }

                        tempAbnormalityAbsenceViewModel.Details = tempDetailsList.ToArray();

                        if (tempAbnormalityAbsenceViewModel.Details.Length == 0)
                        {
                            //CHANGE DOCUMENT APPROVAL TO LOCKED STATUS
                            DocumentApproval tempDocumentApproval = DocumentApprovalRepository.Fetch().Where(x => x.Id == abnormalityAbsenceDocumentApprovalId).FirstOrDefault();

                            tempDocumentApproval.DocumentStatusCode = DocumentStatus.Locked;

                            DocumentApprovalRepository.Upsert<Guid>(tempDocumentApproval, new[] { "DocumentStatusCode" });
                        }
                        else
                        {
                            tempDocumentRequestDetail.ObjectValue = JsonConvert.SerializeObject(tempAbnormalityAbsenceViewModel);
                            DocumentRequestDetailRepository.Upsert<Guid>(tempDocumentRequestDetail, new[] { "ObjectValue" });
                        }
                    }
                }
            #endregion

            #region Abnormality BDJK
                IRepository<AbnormalityBdjk> AbnormalityBdjkRepository = UnitOfWork.GetRepository<AbnormalityBdjk>();

                List<AbnormalityTemp> listAbnormalityBdjkDate = AbnormalityBdjkRepository.Fetch().Where(x => (x.WorkingDate < abnormalityStartDate || x.WorkingDate > abnormalityEndDate) && x.Status == "Progress").Select(x => new AbnormalityTemp() { WorkingDate = x.WorkingDate, NoReg = x.NoReg, Id = x.Id }).ToList();
                var listAbnormalityBdjkDocumentApprovalId = AbnormalityBdjkRepository.Fetch().Where(x => (x.WorkingDate < abnormalityStartDate || x.WorkingDate > abnormalityEndDate) && x.Status == "Progress").Select(x => x.DocumentApprovalId);

                if(listAbnormalityBdjkDate.Count > 0)
                {
                    foreach (Guid abnormalityBdjkDocumentApprovalId in listAbnormalityBdjkDocumentApprovalId)
                    {
                      DocumentRequestDetail tempDocumentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == abnormalityBdjkDocumentApprovalId).FirstOrDefault();

                      AbnormalityBdjkViewModel tempAbnormalityBdjkViewModel = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(tempDocumentRequestDetail.ObjectValue);

                      var tempDetailsList = tempAbnormalityBdjkViewModel.Details.ToList();

                      //DELETE ABNORMALITY Bdjk ROW
                      foreach (AbnormalityBdjkView abnormalityBdjkView in tempAbnormalityBdjkViewModel.Details)
                      {
                          AbnormalityBdjk tempAbnormalityBdjk = AbnormalityBdjkRepository.FindById(abnormalityBdjkView.AbnormalityBdjkId);
                          if (listAbnormalityBdjkDate.Where(x => x.Id == tempAbnormalityBdjk.Id).Count() > 0)
                          {
                              tempAbnormalityBdjk.Status = "Locked";

                              AbnormalityBdjkRepository.Upsert<Guid>(tempAbnormalityBdjk, new[] { "Status" });

                              tempDetailsList.Remove(abnormalityBdjkView);
                          }
                      }

                      tempAbnormalityBdjkViewModel.Details = tempDetailsList.ToArray();

                      if (tempAbnormalityBdjkViewModel.Details.Length == 0)
                      {
                          //CHANGE DOCUMENT APPROVAL TO LOCKED STATUS
                          DocumentApproval tempDocumentApproval = DocumentApprovalRepository.Fetch().Where(x => x.Id == abnormalityBdjkDocumentApprovalId).FirstOrDefault();

                          tempDocumentApproval.DocumentStatusCode = DocumentStatus.Locked;

                          DocumentApprovalRepository.Upsert<Guid>(tempDocumentApproval, new[] { "DocumentStatusCode" });
                      }
                      else
                      {
                          tempDocumentRequestDetail.ObjectValue = JsonConvert.SerializeObject(tempAbnormalityBdjkViewModel);

                          DocumentRequestDetailRepository.Upsert<Guid>(tempDocumentRequestDetail, new[] { "ObjectValue" });
                      }
                  }
                }
            #endregion

            #region Abnormality OverTime
                IRepository<AbnormalityOverTime> AbnormalityOverTimeRepository = UnitOfWork.GetRepository<AbnormalityOverTime>();

                List<AbnormalityTemp> listAbnormalityOverTimeDate = AbnormalityOverTimeRepository.Fetch().Where(x => (x.OvertimeDate < abnormalityStartDate || x.OvertimeDate > abnormalityEndDate) && x.Status == "Progress" && x.DocumentApprovalId != Guid.Empty).Select(x => new AbnormalityTemp() { WorkingDate = x.OvertimeDate, NoReg = x.NoReg, Id = x.Id }).ToList();
                var listAbnormalityOverTimeDocumentApprovalId = AbnormalityOverTimeRepository.Fetch().Where(x => (x.OvertimeDate < abnormalityStartDate || x.OvertimeDate > abnormalityEndDate) && x.Status == "Progress" && x.DocumentApprovalId != Guid.Empty).Select(x => x.DocumentApprovalId);

                if (listAbnormalityOverTimeDate.Count > 0)
                {
                    foreach (Guid abnormalityOverTimeDocumentApprovalId in listAbnormalityOverTimeDocumentApprovalId)
                    {
                        DocumentRequestDetail tempDocumentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == abnormalityOverTimeDocumentApprovalId).FirstOrDefault();

                        AbnormalityOverTimeViewModel tempAbnormalityOverTimeViewModel = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(tempDocumentRequestDetail.ObjectValue);

                        var tempDetailsList = tempAbnormalityOverTimeViewModel.Details.ToList();

                        //DELETE ABNORMALITY OverTime ROW
                        foreach (AbnormalityOverTimeView abnormalityOverTimeView in tempAbnormalityOverTimeViewModel.Details)
                        {
                            AbnormalityOverTime tempAbnormalityOverTime = AbnormalityOverTimeRepository.FindById(abnormalityOverTimeView.AbnormalityOverTimeId);
                            if (listAbnormalityOverTimeDate.Where(x => x.Id == tempAbnormalityOverTime.Id).Count() > 0)
                            {
                                tempAbnormalityOverTime.Status = "Locked";

                                AbnormalityOverTimeRepository.Upsert<Guid>(tempAbnormalityOverTime, new[] { "Status" });

                                tempDetailsList.Remove(abnormalityOverTimeView);
                            }
                        }

                        tempAbnormalityOverTimeViewModel.Details = tempDetailsList.ToArray();

                        if (tempAbnormalityOverTimeViewModel.Details.Length == 0)
                        {
                            //CHANGE DOCUMENT APPROVAL TO LOCKED STATUS
                            DocumentApproval tempDocumentApproval = DocumentApprovalRepository.Fetch().Where(x => x.Id == abnormalityOverTimeDocumentApprovalId).FirstOrDefault();

                            tempDocumentApproval.DocumentStatusCode = DocumentStatus.Locked;

                            DocumentApprovalRepository.Upsert<Guid>(tempDocumentApproval, new[] { "DocumentStatusCode" });
                        }
                        else
                        {
                            tempDocumentRequestDetail.ObjectValue = JsonConvert.SerializeObject(tempAbnormalityOverTimeViewModel);

                            DocumentRequestDetailRepository.Upsert<Guid>(tempDocumentRequestDetail, new[] { "ObjectValue" });
                        }
                    }
                }
            #endregion


            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}
