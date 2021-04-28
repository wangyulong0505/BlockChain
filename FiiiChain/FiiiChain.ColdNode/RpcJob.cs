using FiiiChain.Framework;
using FiiiChain.Wallet.API;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FiiiChain.ColdNode
{
    public class RpcJob : BaseJob
    {
        bool isRunning;
        public override JobStatus Status
        {
            get
            {
                if (!isRunning)
                {
                    return JobStatus.Stopped;
                }
                else
                {
                    return JobStatus.Running;
                }
            }
        }

        public override void Start()
        {
            Task.Run(() => {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);

           
                
                Startup.EngineStopAction = BlockchainJob.Current.Stop;
                Startup.GetEngineJobStatusFunc = BlockchainJob.Current.GetJobStatus;

                var nodePort = GlobalParameters.IsTestnet ? Resources.TestnetNodePort : Resources.MainnetNodePort;
                string url = string.Format("http://*:{0}", nodePort);

                var host = WebHost
                  .CreateDefaultBuilder()
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.ClearProviders();
                    })
                  .UseKestrel().ConfigureServices(cssc => cssc.AddMemoryCache())
                  .UseStartup<Startup>()
                  .UseUrls(url)
                  .Build();
                host.Start();
            });

            isRunning = true;
        }

        public override void Stop()
        {
            isRunning = false;
        }
    }
}
