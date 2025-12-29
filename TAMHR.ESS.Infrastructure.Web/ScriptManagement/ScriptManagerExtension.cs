using Microsoft.Extensions.DependencyInjection;
using System;

namespace TAMHR.ESS.Infrastructure.Web.ScriptManagement
{
    public static class ScriptManagerExtension
    {
        public static IServiceCollection RegisterScriptManager(this IServiceCollection services, Action<ScriptManagerOption> options)
        {
            services.Configure(options);

            services.AddScoped<IScriptManager, ScriptManager>();

            return services;
        }
    }
}
