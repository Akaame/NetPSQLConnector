using System;
using Microsoft.Extensions.Configuration;

namespace NPSQLConnector.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration conf = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var hostName = conf.GetSection("PSQL_HOST");
            var port = conf.GetSection("PSQL_PORT");
            Connector cn = new Connector();
            cn.HostName = hostName.Exists() ? hostName.Value : "localhost";
            cn.Port = port.Exists() ? Convert.ToInt32(port.Value) : 5432;
            cn.UserName = "postgres";
            cn.Password="postgres";
            cn.InitialCatalog = "postgres";
            
            cn.Connect();
            Span<byte> nonce = cn.Startup(); // Seriously?
            cn.AuthMD5(nonce);
            cn.SendSimpleQuery("CREATE TABLE dummy1(id INTEGER);");
        }
    }
}
