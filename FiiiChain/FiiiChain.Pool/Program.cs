using FiiiChain.Framework;
using FiiiChain.KafkaMQ;
using FiiiChain.Pool.Apis;
using FiiiChain.Pool.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace FiiiChain.Pool
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isTestNet = false;
            try
            {
                if (args[0].ToLower().Trim() == "-testnet")
                {
                    isTestNet = true;
                }
            }
            catch
            {

            }
            GlobalParameters.IsTestnet = isTestNet;
            Init();
            Start();
            Console.ReadKey();
        }

        static void Start()
        {
            Task.Run(() =>
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
                    LogHelper.Error(ex.Message, ex);
                }
            });
            Console.WriteLine("Pool Start !!!");
        }

        static void Init()
        {
            RabbitMQApi.Current = new RabbitMQApi();
        }
    }
}
