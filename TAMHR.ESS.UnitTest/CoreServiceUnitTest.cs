using FluentFTP;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;

namespace TAMHR.ESS.UnitTest
{
    public class CoreServiceUnitTest
    {
        [Fact]
        public void TestListing()
        {
            var uri = new Uri("ftp://127.0.0.1/2020/5");

            var host = uri.Host;
            var localPath = uri.LocalPath;
            var fileName = "100664-05-2020-offcycle-\\d*.pdf";
            var dir = localPath.GetFtpDirectoryName();

            var combineUri = new Uri(uri, fileName).AbsolutePath;

            // create an FTP client
            var client = new FtpClient(host, new NetworkCredential("payslipuser", "Transformer6hap"));

            client.Connect();

            var list = client.GetListing(localPath);

            var regex = new Regex(fileName);

            var selectedList = list.Where(x => x.Type == FtpFileSystemObjectType.File && regex.IsMatch(x.Name));

            foreach (var file in selectedList)
            {
                using (var memoryStream = new MemoryStream())
                {
                    client.Download(memoryStream, file.FullName);

                    var bytes = memoryStream.ToArray();
                }
            }

            client.Disconnect();
        }
    }
}
