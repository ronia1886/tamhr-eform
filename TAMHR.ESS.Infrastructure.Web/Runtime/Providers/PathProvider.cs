using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace TAMHR.ESS.Infrastructure.Web.Runtime.Providers
{
    public class PathProvider
    {
        private IHostingEnvironment _environment;

        public PathProvider(IHostingEnvironment environmnet)
        {
            _environment = environmnet;
        }

        public string FilePath(string path)
        {
            return Path.Combine(_environment.ContentRootPath, path);
        }

        public string ContentPath(string path)
        {
            return Path.Combine(_environment.WebRootPath, path);
        }
    }
}
