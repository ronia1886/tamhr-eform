using Agit.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Configurations;
using TAMHR.ESS.Infrastructure.Web.DomainEvent;

namespace TAMHR.ESS.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        //}
        public void ConfigureServices(IServiceCollection services)
        {
            var OtherConfigList = Configuration.GetSection("OtherConfiguration").Get<List<string>>();
            string securityKey = "";
            if (OtherConfigList.Count > 0)
            {
                securityKey = OtherConfigList[0];
            }
           
            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //what validate
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        //setup validate data
                        ValidIssuer = "rahadian",
                        ValidAudience = "rahadian",
                        IssuerSigningKey = symetricSecurityKey
                    };
                });
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var connection = Configuration["ConnectionString"];
            services.AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(connection));

            //Configure application configuration
            services.Configure<ApplicationConfiguration>(config =>
            {
                config.Version = Configuration["Version"];
                config.ConnectionString = Configuration["ConnectionString"];
            });

            //Register Domain Events
            services.AddDomainEvents();

            services.AddTransient<CoreService>();
            services.AddTransient<ConfigService>();
            services.AddTransient<UserService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Some code omitted for brevity...
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            //app.UseMvc();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
