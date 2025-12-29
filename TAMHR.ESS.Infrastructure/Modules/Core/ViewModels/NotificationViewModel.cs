using System.Collections.Generic;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class NotificationViewModel
    {
        public int Total { get; set; }
        public IEnumerable<object> Messages { get; set; }
    }
}
