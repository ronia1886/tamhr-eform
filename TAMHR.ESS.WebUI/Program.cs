using Agit.Domain;
using Agit.Domain.UnitOfWork;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NSwag.AspNetCore;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Validators;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.BackgroundTask;
using TAMHR.ESS.Infrastructure.Web.DomainEvent;
using TAMHR.ESS.Infrastructure.Web.Filters;
using TAMHR.ESS.Infrastructure.Web.Helpers;
using TAMHR.ESS.Infrastructure.Web.Localization;
using TAMHR.ESS.Infrastructure.Web.Memory;
using TAMHR.ESS.Infrastructure.Web.Middlewares;
using TAMHR.ESS.Infrastructure.Web.Mvc;
using TAMHR.ESS.Infrastructure.Web.Pwa;
using TAMHR.ESS.Infrastructure.Web.Pwa.ServiceWorker;
using TAMHR.ESS.Infrastructure.Web.Runtime.Compressions;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using TAMHR.ESS.Infrastructure.Web.ScriptManagement;
using TAMHR.ESS.WebUI;
using TAMHR.ESS.WebUI.Areas.OHS.ChatHub;
using TAMHR.ESS.WebUI.Areas.OHS.State;

ExcelPackage.License.SetNonCommercialOrganization("TAMHR");

var builder = WebApplication.CreateBuilder(args);

// === LOGGING PROVIDERS (Console + Debug) ===
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Layanan MVC, JSON, dan Localization
builder.Services.AddControllersWithViews()
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
    // Mengatur Contract Resolver (misalnya untuk PascalCase)
    //options.SerializerSettings.ContractResolver = new DefaultContractResolver();

    // Mengatur bagaimana properti kosong/null ditangani
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Contoh: Abaikan properti null saat serialisasi
    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include; // Contoh: Sertakan nilai default

    // Mengatur format tanggal
    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss"; // Contoh: ISO 8601 UTC
    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

    // Mengubah policy casing menjadi Null (tidak ada perubahan casing)
    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    //options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter());
    //options.JsonSerializerOptions.Converters.Add(new NullableBoolConverter());
    //options.JsonSerializerOptions.Converters.Add(new NullableIntConverter());
    //options.JsonSerializerOptions.Converters.Add(new NullableDecimalConverter());
    //options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
});

builder.Services.AddRazorPages();

builder.Services.AddValidatorsFromAssemblyContaining<TAMHR.ESS.Infrastructure.UnitOfWork>();

builder.Services.AddSingleton<IStringLocalizerFactory, EntityFrameworkLocalizerFactory>();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true; // Opsional, tapi disarankan
})
                .AddFluentValidationClientsideAdapters(); // Opsional untuk client-side (jQuery Validate)



builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("id-ID")
    };

    var defaultCulture = builder.Configuration["DefaultCulture"];

    // Fallback ke id-ID jika defaultCulture kosong / tidak cocok
    if (string.IsNullOrWhiteSpace(defaultCulture) ||
        !supportedCultures.Any(c => c.Name.Equals(defaultCulture, StringComparison.OrdinalIgnoreCase)))
    {
        defaultCulture = "id-ID";
    }

    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Konfigurasi Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});


builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IUrlHelper>(factory =>
{
    var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
    return new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(actionContext);
});

// Pendaftaran Path Provider
builder.Services.AddPathProvider();

// Konfigurasi Localization
builder.Services.AddLocalization();

builder.Services.AddMvc(options =>
{
    options.Filters.Add<ExceptionFilter>();
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
})
    .AddViewLocalization();

// Pendaftaran SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddSession();

// Pendaftaran Kompresi
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add(new BrotliCompressionProvider());
});

// Pendaftaran DbContext
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connection))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// === VERSI LAMA (DISIMPAN SEBAGAI KOMENTAR) ===
// builder.Services.AddDbContext<UnitOfWork>(options => options.UseSqlServer(connection));

// === VERSI DENGAN LOGGING EF CORE ===
builder.Services.AddDbContext<UnitOfWork>(options =>
{
    options.UseSqlServer(connection);

#if DEBUG
    // Tampilkan error detail & nilai parameter SQL (hati-hati di production)
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();

    // Log perintah SQL ke Output (Debug) / Console
    options.LogTo(
        msg => Debug.WriteLine(msg),                                   // ke Output VS (Debug)
        new[] { DbLoggerCategory.Database.Command.Name },              // fokus ke SQL command
        LogLevel.Information                                           // level log
    );
#endif
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<DomainEventManager>();

// Pendaftaran Layanan Domain dengan Scrutor
builder.Services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
    .AddClasses(classes => classes.AssignableTo<DomainServiceBase>())
    .AsSelf()
    .WithTransientLifetime()
);

// Pendaftaran Script Manager
builder.Services.RegisterScriptManager(options =>
{
    var scriptOptions = JsonConvert.DeserializeObject<ScriptManagerOption>(File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, "jsdependencies.json")));
    options.Bundle = scriptOptions.Bundle;
    options.DependencyPath = scriptOptions.DependencyPath;
    options.ReferenceItems = scriptOptions.ReferenceItems;
});

// Pendaftaran Utilitas
builder.Services.AddMemoryCache();

// Pendaftaran CORS
builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        var appUrl = builder.Configuration["AppUrl"];
        policyBuilder.AllowAnyMethod()
               .AllowAnyHeader()
               .WithOrigins(appUrl)
               .AllowCredentials();
    })
);

// Konfigurasi Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        var maintenance = bool.Parse(builder.Configuration["Maintenance"]);
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.Name = "TAMHR.ESS" + (builder.Environment.IsDevelopment() ? ".DEV" : string.Empty);
        //options.Cookie.HttpOnly = bool.Parse(builder.Configuration["Authentication:HttpOnly"]);
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(builder.Configuration["Authentication:ExpireTimeSpan"]));
        options.SlidingExpiration = bool.Parse(builder.Configuration["Authentication:SlidingExpiration"]);

        var path = builder.Configuration[maintenance ? "MaintenanceHandler" : "Authentication:LoginPath"];
        options.LoginPath = path;
        options.AccessDeniedPath = options.LoginPath;
        options.LogoutPath = builder.Configuration["Authentication:LogoutPath"];
        options.ReturnUrlParameter = builder.Configuration["Authentication:ReturnUrlParameter"];

        var useMemoryCacheSessionStore = bool.Parse(builder.Configuration["Authentication:MemoryCacheSessionStore"]);
        if (useMemoryCacheSessionStore)
        {
            options.SessionStore = new MemoryCacheTicketStore(options);
        }
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken"; // This must match the JS header
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ HTTPS only
});

//Register SignalR
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = false;
});

// Pendaftaran Acl Helper
builder.Services.AddScoped<AclHelper>();

// Pendaftaran Kendo UI
builder.Services.AddKendo();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Allowed", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

//// Pendaftaran PWA
//var registerServiceWorker = bool.Parse(builder.Configuration["RegisterServiceWorker"]);
//if (registerServiceWorker)
//{
//    builder.Services.AddProgressiveWebApp(new PwaOptions
//    {
//        CacheId = "v1.2.4",
//        Strategy = ServiceWorkerStrategy.NetworkFirst,
//        RoutesToPreCache = "/",
//        RegisterServiceWorker = true
//    });
//}

//// Pendaftaran Domain Events
builder.Services.AddDomainEvents();

//// Pendaftaran Background Task
//builder.Services.AddHostedService<QueuedHostedService>();
//builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// === PENTING: Panggil Build() hanya sekali setelah semua layanan terdaftar ===
var app = builder.Build();

// Middleware Error Handling
var errorHandler = builder.Configuration["ErrorHandler"];
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwaggerUiWithApiExplorer(settings =>
    //{
    //    settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
    //});
}
else
{
    app.UseExceptionHandler("/Error");
}

// Middleware Utama
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// === ENABLE REQUEST LOCALIZATION (IMPORTANT for culture cookie to work) ===
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// Middleware Autentikasi dan Otorisasi (urutan ini sangat penting)
app.UseAuthentication();
app.UseAuthorization();

// Middleware Cookie Policy
var useCookiePolicy = bool.Parse(builder.Configuration["UseCookiePolicy"]);
if (useCookiePolicy)
{
    app.UseCookiePolicy();
}

// Middleware Keamanan Header
app.Use(async (context, next) =>
{
    if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
    {
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    }
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'self'");
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("X-HTTP-Method-Override") ||
    context.Request.Headers.ContainsKey("X-Method-Override") ||
    context.Request.Headers.ContainsKey("X-HTTP-Method") ||
        context.Request.Query.ContainsKey("_method"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("HTTP Method Override Not Allowed");
        return;
    }
    await next();
});

// Middleware tambahan
app.UseCors("CorsPolicy");
//app.UseResponseCompression();

// === Konfigurasi Endpoint (Map Routings) ===
app.MapHub<ChatHub>("/chatHub");
app.MapHub<TanyaOhsChatHub>("/tanyaOhs");

// Konfigurasi Rute MVC (Ganti UseMvc dengan Map...)
app.MapControllers(); // Maps controller endpoints
app.MapAreaControllerRoute(
    name: "ClaimBenefitArea",
    areaName: "ClaimBenefit",
    pattern: "claimbenefit/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "CoreArea",
    areaName: "Core",
    pattern: "core/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "MasterDataArea",
    areaName: "MasterData",
    pattern: "masterdata/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "OHSArea",
    areaName: "OHS",
    pattern: "ohs/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "OthersArea",
    areaName: "Others",
    pattern: "others/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "PersonalDataArea",
    areaName: "PersonalData",
    pattern: "personaldata/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "PersonalDataArea",
    areaName: "PersonalData",
    pattern: "personaldata/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "ReportingArea",
    areaName: "Reporting",
    pattern: "reporting/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
    name: "TimeManagementArea",
    areaName: "TimeManagement",
    pattern: "timemanagement/{controller=Home}/{action=Index}");

app.MapControllerRoute(
    name: "global",
    pattern: "{controller}/{action=Index}");
app.MapControllerRoute(
    name: "news",
    pattern: "news/{slug}",
    defaults: new { area = "Core", controller = "News", action = "Detail" });
app.MapAreaControllerRoute(
    name: "areas",
    areaName: "{area}",
    pattern: "{area:exists}/{controller}/{action}");
app.MapAreaControllerRoute(
    name: "default",
    areaName: "Core",
    pattern: "{area=Core}/{controller=Default}/{action=Index}");
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}");

app.Run();