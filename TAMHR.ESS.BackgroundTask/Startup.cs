using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TAMHR.ESS.BackgroundTask
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            //services.AddMvc();
            services.AddControllers();
            services.AddSingleton<IWorker, Worker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add these lines
            var loggingOptions = this.Configuration.GetSection("Log4NetCore")
                                                   .Get<Log4NetProviderOptions>();
            loggerFactory.AddLog4Net(loggingOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();   // ✅ pastikan API bisa jalan
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
        }
    }
}
