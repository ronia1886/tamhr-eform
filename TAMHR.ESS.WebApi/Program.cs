
using Agit.Domain;
using Agit.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    // Mengubah policy casing menjadi Null (tidak ada perubahan casing)
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableBoolConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableIntConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDecimalConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
});

// Tambahkan layanan ke container
builder.Services.AddControllersWithViews();

// Gunakan UseIISIntegration untuk mengkonfigurasi aplikasi agar berjalan dengan IIS
builder.WebHost.UseIISIntegration();

// Get and set connection string from configuration
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connection))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Register your DbContext correctly.
// The type argument must be a class inheriting from DbContext.
builder.Services.AddDbContext<UnitOfWork>(options =>
    options.UseSqlServer(connection)); // Replace with your actual connection string

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Scan assembly and register classes that implement domain service base class
builder.Services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
    .AddClasses(classes =>
    {
        classes.AssignableTo<DomainServiceBase>();
    })
    .AsSelf()
    .WithTransientLifetime()
);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(Agit.Domain.UnitOfWork.IUnitOfWork));
    });

// Dapatkan Secret Key dari konfigurasi
var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    builder.Configuration["securityKey"] ?? throw new InvalidOperationException("securityKey not found.")
));

// Tambahkan layanan otentikasi
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Praktik terbaik: Validasi waktu hidup token
            ValidateIssuerSigningKey = true, // Praktik terbaik: Validasi kunci
            ValidIssuer = "rahadian",
            ValidAudience = "rahadian",
            IssuerSigningKey = symetricSecurityKey
        };
    });

var app = builder.Build();

// Konfigurasi HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
//using Microsoft.AspNetCore;
//using Microsoft.AspNetCore.Hosting;

//namespace TAMHR.ESS.WebApi
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            BuildWebHost(args).Run();
//        }

//        public static IWebHost BuildWebHost(string[] args) =>
//            WebHost.CreateDefaultBuilder(args)
//                .UseKestrel()
//                .UseStartup<Startup>()
//                .UseIISIntegration()
//                .Build();
//    }
//}
