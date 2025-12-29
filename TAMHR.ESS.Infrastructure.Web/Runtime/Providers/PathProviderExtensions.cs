using Microsoft.Extensions.DependencyInjection;

namespace TAMHR.ESS.Infrastructure.Web.Runtime.Providers
{
    public static class PathProviderExtensions
    {
        public static IServiceCollection AddPathProvider(this IServiceCollection services)
        {
            services.AddSingleton<PathProvider>();

            return services;
        }
    }
}
