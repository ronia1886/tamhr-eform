using System;
using Agit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TAMHR.ESS.Infrastructure.Web.Models;

namespace TAMHR.ESS.Infrastructure.Extensions
{
    public static class RouteDataExtensions
    {
        public static RouteInfo Extract(this RouteData routeData)
        {
            var area = (string)routeData.Values["area"];
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            var routeInfo = new RouteInfo(area, controller, action, false);

            return routeInfo;
        }
    }
}
