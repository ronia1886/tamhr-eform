using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace TAMHR.ESS.EmailReminderService
{
    public static class QuartzHostedServiceCollectionExtensions
    {

        public static IServiceCollection AddQuartzHostedService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            

            services.AddSingleton<IJobFactory, CoreJobFactory>();
            services.AddSingleton(provider =>
            {
                var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },

                };

                var sf = new StdSchedulerFactory(props);
                var scheduler = sf.GetScheduler().GetAwaiter().GetResult();
                scheduler.JobFactory = provider.GetService<IJobFactory>();

                return scheduler;
            });

            services.AddHostedService<QuartzHostedService>();

            return services;
        }
    }
}
