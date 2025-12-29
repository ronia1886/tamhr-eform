using Agit.Domain;
using Agit.Domain.Event;
using Microsoft.Extensions.DependencyInjection;
using TAMHR.ESS.Infrastructure.DomainEvents;

namespace TAMHR.ESS.Infrastructure.Web.DomainEvent
{
    public static class DomainEventExtensions
    {
        public static IServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            //services.AddSingleton<DomainEventManager>();

            services.AddTransient<IDomainEventHandler<DocumentApprovalEvent>, DocumentApprovalEventHandler>();

            return services;
        }
    }
}
