using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data.SqlClient;

namespace TAMHR.ESS.WebUI.State
{
    public class Db
    {
        public static IConfiguration AppConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public static string ConnectionString = AppConfig.GetValue<string>("ConnectionStrings:DefaultConnection");
        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
