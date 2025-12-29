using Microsoft.Extensions.DependencyInjection;

namespace TAMHR.ESS.Infrastructure.Web.Authorization
{
    public static class AclExtensions
    {
        public static IServiceCollection AddAclHelper(this IServiceCollection services)
        {
            services.AddScoped<AclHelper>();

            return services;
        }
    }
}
