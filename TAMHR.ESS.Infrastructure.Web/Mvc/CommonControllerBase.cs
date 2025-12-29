using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web
{
    public abstract class CommonControllerBase : Controller
    {
        protected readonly ServiceProxy ServiceProxy;

        protected AclHelper AclHelper => ServiceProxy.GetAclHelper();

        /// <summary>
        /// Config service
        /// </summary>
        public ConfigService ConfigService => ServiceProxy.GetService<ConfigService>();

        public CommonControllerBase() => ServiceProxy = new ServiceProxy(this);
    }
}
