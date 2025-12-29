using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web.Pwa;
using TAMHR.ESS.Infrastructure.Web.Filters;
using TAMHR.ESS.Infrastructure.Web.DomainEvent;
using TAMHR.ESS.Infrastructure.Web.Localization;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.ScriptManagement;
using TAMHR.ESS.Infrastructure.Web.Pwa.ServiceWorker;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using TAMHR.ESS.Infrastructure.Web.Runtime.Compressions;
using Agit.Domain;
using Agit.Domain.UnitOfWork;
using NJsonSchema;
using Newtonsoft.Json;
using NSwag.AspNetCore;
using Rotativa.AspNetCore;
using FluentValidation.AspNetCore;
using Newtonsoft.Json.Serialization;
using TAMHR.ESS.Infrastructure.Web.Configurations;
using TAMHR.ESS.Infrastructure.Web.Middlewares;
using TAMHR.ESS.Infrastructure.Web.Memory;
using TAMHR.ESS.Infrastructure.Web.BackgroundTask;
using TAMHR.ESS.WebUI.Areas.OHS.State;
using TAMHR.ESS.WebUI.Areas.OHS.ChatHub;

namespace TAMHR.ESS
{
    public class ChatHub : Hub
    {
        public async Task ReceiveMessage(string channelId, string noreg, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", channelId, noreg, message);
        }

        public async Task AssociateJob(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Global.StaticConfig = configuration;
            Environment = environment;
        }

        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Configure localization
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ja-JP"),
                    new CultureInfo("id-ID"),
                    new CultureInfo("zh-CN")
                };

                var defaultCulture = Configuration["DefaultCulture"];

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            //Configure cookie policy
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });

            //Register Action Context Accessor and Url Helper
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;

                return new UrlHelper(actionContext);
            });

            //Register path provider
            services.AddPathProvider();

            //Register cookie based Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    var maintenance = bool.Parse(Configuration["Maintenance"]);

                    options.Cookie.Name = "TAMHR.ESS" + (Environment.IsDevelopment() ? ".DEV" : string.Empty);
                    //options.Cookie.HttpOnly = bool.Parse(Configuration["Authentication:HttpOnly"]);
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(Configuration["Authentication:ExpireTimeSpan"]));
                    options.SlidingExpiration = bool.Parse(Configuration["Authentication:SlidingExpiration"]);

                    var path = Configuration[maintenance ? "MaintenanceHandler" : "Authentication:LoginPath"];

                    options.LoginPath = path;
                    options.LogoutPath = Configuration["Authentication:LogoutPath"];
                    options.AccessDeniedPath = options.LoginPath;
                    options.ReturnUrlParameter = Configuration["Authentication:ReturnUrlParameter"];

                    var useMemoryCacheSessionStore = bool.Parse(Configuration["Authentication:MemoryCacheSessionStore"]);

                    if (useMemoryCacheSessionStore)
                    {
                        options.SessionStore = new MemoryCacheTicketStore(options);
                    }
                });

            //Register Memory Cache
            services.AddMemoryCache();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken"; // This must match the JS header
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ HTTPS only
            });


            //Register CORS
            services.AddCors(options =>
                options.AddPolicy("CorsPolicy", builder =>
                    {
                        var appUrl = Configuration["AppUrl"];

                        builder.AllowAnyMethod()
                               .AllowAnyHeader()
                               .WithOrigins(appUrl)
                               .AllowCredentials();
                    }
                )
            );

            //Register Localization
            services.AddLocalization();
            services.AddSingleton<IStringLocalizerFactory, EntityFrameworkLocalizerFactory>();

            //Register MVC, Validators and Some Action Filters
            services.AddMvc(options =>
            {
                var ssoSessionFilter = bool.Parse(Configuration["SsoSessionFilter"]);

                options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();

                if (ssoSessionFilter)
                {
                    options.Filters.Add<SsoSessionFilter>();
                }

                options.Filters.Add<ExceptionFilter>();
            })
            .AddViewLocalization()
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<UnitOfWork>())
            .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            //Register SignalR
            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            });

            //Register Compression Middleware
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add(new BrotliCompressionProvider());
            });

            var connection = Configuration["ConnectionString"];
            services.AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(connection));

            //Configure application configuration
            services.Configure<ApplicationConfiguration>(config =>
            {
                config.Version = Configuration["Version"];
                config.ConnectionString = Configuration["ConnectionString"];
            });
            
            //Scan assembly and register classes that implement domain service base class
            services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
                .AddClasses(classes =>
                {
                    classes.AssignableTo<DomainServiceBase>();
                })
                .AsSelf()
                .WithTransientLifetime()
            );

            //Register Script Manager
            services.RegisterScriptManager(options =>
            {
                var scriptOptions = JsonConvert.DeserializeObject<ScriptManagerOption>(File.ReadAllText(Path.Combine(Environment.ContentRootPath, "jsdependencies.json")));

                options.Bundle = scriptOptions.Bundle;
                options.DependencyPath = scriptOptions.DependencyPath;
                options.ReferenceItems = scriptOptions.ReferenceItems;
            });

            //Register Acl Helper
            services.AddAclHelper();

            //Register Kendo Service
            services.AddKendo();

            //Register PWA
            var registerServiceWorker = bool.Parse(Configuration["RegisterServiceWorker"]);

            if (registerServiceWorker)
            {
                services.AddProgressiveWebApp(new PwaOptions
                {
                    CacheId = "v1.2.4",
                    Strategy = ServiceWorkerStrategy.NetworkFirst,
                    RoutesToPreCache = "/",
                    RegisterServiceWorker = true
                });
            }

            //Register Domain Events
            services.AddDomainEvents();

            //Register Queue Hosted Service
            services.AddHostedService<QueuedHostedService>();
            
            //Register Background Task Queue
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);
            var errorHandler = Configuration["ErrorHandler"];
            var useCookiePolicy = bool.Parse(Configuration["UseCookiePolicy"]);

            if (Environment.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseExceptionMiddleware(errorHandler);

                app.UseSwaggerUiWithApiExplorer(settings =>
                {
                    settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                });
            }
            else
            {
                app.UseExceptionMiddleware(errorHandler);
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseHttpsRedirection();

            if (useCookiePolicy)
            {
                app.UseCookiePolicy();
            }

            app.Use(async (context, next) =>
            {
                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN"); // or "DENY"
                }

                context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'self'");

                await next();
            });

            app.UseCors("CorsPolicy");
            app.UseResponseCompression();
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
                routes.MapHub<TanyaOhsChatHub>("/tanyaOhs");
            });

             app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "global",
                    template: "{controller}/{action=Index}");

                routes.MapRoute(
                    name: "news",
                    template: "news/{slug}",
                    defaults: new { area = "Core", controller = "News", action = "Detail" });

                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller}/{action}");

                routes.MapRoute(
                    name: "default",
                    template: "{area=Core}/{controller=Default}/{action=Index}");

                routes.MapRoute(
                    name: "api",
                    template: "api/{controller}/{action}");
            });

            RotativaConfiguration.Setup(Environment);
        }
    }
}
