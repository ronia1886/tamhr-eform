using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using TAMHR.ESS.Infrastructure.Web.Querying;
using TAMHR.ESS.Infrastructure.Web.Localization;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.ContextPrincipal;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using Agit.Domain;
using Agit.Common.Cache;
using Agit.Common.Utility;
using Agit.Domain.UnitOfWork;
using Kendo.Mvc.UI;
using Microsoft.Extensions.Options;
using TAMHR.ESS.Infrastructure.Web.BackgroundTask;
using Microsoft.Extensions.Hosting;

namespace TAMHR.ESS.Infrastructure.Web
{
    /// <summary>
    /// Class that handle creation of service object, access control list helper, and user claim 
    /// </summary>
    public class ServiceProxy
    {
        /// <summary>
        /// Generic cache object
        /// </summary>
        private readonly GenericCache _genericCaches = new GenericCache();

        /// <summary>
        /// Base controller object
        /// </summary>
        private ControllerBase _controllerBase;

        /// <summary>
        /// Get user claim from generic cache
        /// </summary>
        public UserClaim UserClaim => _genericCaches.Get(() => UserClaim.CreateFrom(_controllerBase.HttpContext.User));

        /// <summary>
        /// Get application configuration (appsettings.json) from generic cache
        /// </summary>
        public IConfiguration Configuration => _genericCaches.Get(() => (IConfiguration)ServiceProvider.GetService(typeof(IConfiguration)));

        /// <summary>
        /// Service provider object
        /// </summary>
        public IServiceProvider ServiceProvider { get { return _controllerBase.HttpContext.RequestServices; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerBase">Base controller object</param>
        public ServiceProxy(ControllerBase controllerBase)
        {
            _controllerBase = controllerBase;
        }

        /// <summary>
        /// Get background task queue service.
        /// </summary>
        /// <returns>This <see cref="IBackgroundTaskQueue"/> concrete object.</returns>
        public IBackgroundTaskQueue GetBackgroundTaskQueue() => (IBackgroundTaskQueue)ServiceProvider.GetService(typeof(IBackgroundTaskQueue));

        /// <summary>
        /// Get localizer factory from service provider.
        /// </summary>
        /// <returns>This <see cref="EntityFrameworkLocalizerFactory"/> object.</returns>
        public EntityFrameworkLocalizerFactory GetLocalizerFactory() => (EntityFrameworkLocalizerFactory)ServiceProvider.GetService(typeof(IStringLocalizerFactory));

        /// <summary>
        /// Get anti forgery token object from service provider.
        /// </summary>
        /// <returns>This <see cref="IAntiforgery"/> concrete object.</returns>
        public IAntiforgery GetAntiForgery() => (IAntiforgery)ServiceProvider.GetService(typeof(IAntiforgery));

        /// <summary>
        /// Get hosting environment object from service provider
        /// </summary>
        /// <returns>Hosting Environment Object</returns>
        public IWebHostEnvironment GetHostingEnvironment() => (IWebHostEnvironment)ServiceProvider.GetService(typeof(IWebHostEnvironment));

        /// <summary>
        /// Get http context accessor object from service provider
        /// </summary>
        /// <returns>HTTP Context Accessor Object</returns>
        public IHttpContextAccessor GetHttpContextAccessor() => (IHttpContextAccessor)ServiceProvider.GetService(typeof(IHttpContextAccessor));

        /// <summary>
        /// Get context options from service provider
        /// </summary>
        /// <returns>Db Context Options Object</returns>
        public DbContextOptions<UnitOfWork> GetDbContextOptions() => (DbContextOptions<UnitOfWork>)ServiceProvider.GetService(typeof(DbContextOptions<UnitOfWork>));

        /// <summary>
        /// Get localizer from service provider
        /// </summary>
        /// <returns>Localizer Object</returns>
        public IStringLocalizer<ServiceProxy> GetLocalizer() => _genericCaches.Get(() => (IStringLocalizer<ServiceProxy>)ServiceProvider.GetService(typeof(IStringLocalizer<ServiceProxy>)));

        /// <summary>
        /// Get access control list object from generic cache
        /// </summary>
        /// <returns>Access Control List Object</returns>
        public AclHelper GetAclHelper() => _genericCaches.Get(() => (AclHelper)ServiceProvider.GetService(typeof(AclHelper)));

        /// <summary>
        /// Get path provider object from generic cache
        /// </summary>
        /// <returns>Path Provider Object</returns>
        public PathProvider GetPathProvider() => _genericCaches.Get(() => (PathProvider)ServiceProvider.GetService(typeof(PathProvider)));

        /// <summary>
        /// Get domain service object from generic cache
        /// </summary>
        /// <typeparam name="T">Domain Service Class</typeparam>
        /// <returns>Domain Service Object</returns>
        public T GetService<T>() where T : DomainServiceBase => _genericCaches.Get(() => (T)ServiceProvider.GetService(typeof(T)));

        /// <summary>
        /// Get table valued summary by category
        /// </summary>
        /// <typeparam name="T">Table-Valued Function Class</typeparam>
        /// <param name="field">Comparer Field</param>
        /// <param name="category">General Category</param>
        /// <param name="objectParameters">Object Parameters</param>
        /// <returns>Dynamic Collections</returns>
        public IEnumerable<dynamic> GetTableValuedSummary<T>(string field, string category, object objectParameters = null, string filter = null) where T : class
        {
            var unitOfWork = (UnitOfWork)ServiceProvider.GetService(typeof(IUnitOfWork));
            var genericSummaryBuilder = new GenericSummaryBuilder(unitOfWork);

            return genericSummaryBuilder.BuildSummary<T>(field, category, objectParameters != null ? ObjectHelper.ConvertToDictionary(objectParameters) : null, filter);
        }

        /// <summary>
        /// Get datasource result object from table/view
        /// </summary>
        /// <typeparam name="T">Table/View Class</typeparam>
        /// <param name="request">Datasource Request Object</param>
        /// <param name="objectParameters">Object Parameters</param>
        /// <returns>Datasource Result Object</returns>
        public DataSourceResult GetDataSourceResult<T>(DataSourceRequest request, object objectParameters = null) where T : class
        {
            var unitOfWork = (UnitOfWork)ServiceProvider.GetService(typeof(IUnitOfWork));
            var genericDataQuery = new GenericDataQuery(unitOfWork, request);

            return genericDataQuery.GetFromTable<T>(objectParameters != null ? ObjectHelper.ConvertToDictionary(objectParameters) : null);
        }

        /// <summary>
        /// Get datasource result object from table-valued function
        /// </summary>
        /// <typeparam name="T">Table-Valued Function Class</typeparam>
        /// <param name="request">Datasource Request Object</param>
        /// <param name="objectParameters">Object Parameters</param>
        /// <returns>Datasource Result Object</returns>
        public DataSourceResult GetTableValuedDataSourceResult<T>(DataSourceRequest request, object objectParameters) where T : class
        {
            var unitOfWork = (UnitOfWork)ServiceProvider.GetService(typeof(IUnitOfWork));
            var genericDataQuery = new GenericDataQuery(unitOfWork, request);
            var parameters = ObjectHelper.ConvertToDictionary(objectParameters);

            return genericDataQuery.GetFromTableValued<T>(parameters);
        }

        /// <summary>
        /// Get datasource result object from custom sql query
        /// </summary>
        /// <param name="request">Datasource Request Object</param>
        /// <param name="objectParameters">Object Parameters</param>
        /// <returns>Datasource Result Object</returns>
        public DataSourceResult GetQueryDataSourceResult<T>(string query, DataSourceRequest request, object objectParameters) where T : class
        {
            var unitOfWork = (UnitOfWork)ServiceProvider.GetService(typeof(IUnitOfWork));
            var genericDataQuery = new GenericDataQuery(unitOfWork, request);
            var parameters = ObjectHelper.ConvertToDictionary(objectParameters);

            return genericDataQuery.GetFromQuery<T>(query, parameters);
        }
    }
}
