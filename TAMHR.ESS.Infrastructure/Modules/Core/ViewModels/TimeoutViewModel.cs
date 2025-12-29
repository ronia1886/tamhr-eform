using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class TimeoutViewModel
    {
        public int IdleAfter { get; set; }    
        public int Timeout { get; set; }    
        public int PollingInterval { get; set; }    
        public int FailedRequests { get; set; }

        public static TimeoutViewModel CreateFrom(IEnumerable<Config> configs)
        {
            var timeoutViewModel = new TimeoutViewModel
            {
                IdleAfter = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Timeout.IdleAfter")?.ConfigValue ?? "0"),
                Timeout = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Timeout.Interval")?.ConfigValue ?? "0"),
                PollingInterval = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Timeout.PollingInterval")?.ConfigValue ?? "0"),
                FailedRequests = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Timeout.FailedRequests")?.ConfigValue ?? "0"),
            };

            return timeoutViewModel;
        }
    }
}
