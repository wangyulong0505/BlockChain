using FiiiChain.Framework;
using FiiiChain.NewPool.API;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace FiiiChain.NewPool
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isTestNet = false;
            try
            {
                if (args[0].ToLower().Trim() == "-testnet") { isTestNet = true; }
            }
            catch { }
            GlobalParameters.IsTestnet = isTestNet;
            PoolInit();
            PoolStart();
            Console.ReadKey();
        }

        private static void PoolInit()
        {
            RabbitMQApi.Current = new RabbitMQApi();
            //SocketEntity socket = ReadConfig.SocketConfig;
            //PoolEntity kafka = ReadConfig.PoolConfig;
        }

        private static void PoolStart()
        {
            try
            {
                PoolJob poolJob = new PoolJob();
                poolJob.Start();
                poolJob.StartListen();

                TimerTasks.Current.Init();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Start error:{ex.Message}!");
            }
            Console.WriteLine("Pool Start !!!");
        }
    }
}
