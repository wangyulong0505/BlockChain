using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using FiiiChain.MiningPool.Business;
using FiiiChain.Framework;
using FiiiChain.MiningPool.API.Config;

namespace FiiiChain.MiningPool.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigurationTool tool = new ConfigurationTool();
            ServerSetting setting = tool.GetAppSettings<ServerSetting>("ServerSetting");
            
            GlobalParameters.IsTestnet = setting.IsTestNet;

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
