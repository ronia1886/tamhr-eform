using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Web.Models
{
    public class RouteInfo
    {
        public string Area { get; }
        public string Controller { get; }
        public string Action { get; }
        public bool IsWebApi { get; }

        public RouteInfo(string area, string controller, string action, bool isWebApi)
        {
            Area = area;
            Controller = controller;
            Action = action;
            IsWebApi = isWebApi;
        }
    }
}
