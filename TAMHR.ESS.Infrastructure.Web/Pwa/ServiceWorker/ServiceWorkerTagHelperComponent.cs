using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace TAMHR.ESS.Infrastructure.Web.Pwa.ServiceWorker
{
    internal class ServiceWorkerTagHelperComponent : TagHelperComponent
    {
        private string _script;

        private IHostingEnvironment _env;
        private IHttpContextAccessor _accessor;
        private PwaOptions _options;

        public ServiceWorkerTagHelperComponent(IHostingEnvironment env, IHttpContextAccessor accessor, PwaOptions options, IUrlHelper urlHelper)
        {
            _env = env;
            _accessor = accessor;
            _options = options;
            
            _script = "\r\n\t<script" + (_options.EnableCspNonce ? Constants.CspNonce : string.Empty) + ">'serviceWorker'in navigator&&navigator.serviceWorker.register('" + urlHelper.Content($"~{Constants.ServiceworkerRoute}") + "')</script>";
        }

        /// <inheritdoc />
        public override int Order => -1;

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_options.RegisterServiceWorker)
            {
                return;
            }

            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                if (_accessor.HttpContext.Request.IsHttps || _env.IsDevelopment())
                {
                    output.PostContent.AppendHtml(_script);
                }
            }
        }
    }
}
