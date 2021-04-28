using FiiiChain.Consensus.Api;
using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using FiiiChain.ShareModels.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiChain.NewMinerTest
{
    class Program
    {
        private static readonly object _lock = new object();
        //参数格式 N30001N5013 fiiit5D6zFXn3xKmuAFbM7MQEkCPQGxZgadxE5 F:\Plot 2147483648
        //参数说明 POS机序列号 钱包地址 文件路径 容量(2G)
        static void Main(string[] args)
        {
            GlobalParameters.IsTestnet = true;

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "init":
                        Init(args);
                        break;
                    case "start":
                        StartMining();
                        break;
                    default:
                        UnknownCommand();
                        break;
                }
            }
            else
            {
                UnknownCommand();
            }
        }

        static void UnknownCommand()
        {
            Console.WriteLine("Useage: dotnet MinerTest.dll [command] [options]");
            Console.WriteLine("");
            Console.WriteLine("Supported Commands:");
            Console.WriteLine("     Register: register current device to mining pool");
            Console.WriteLine("     Init: initialize local devie, prepare to start mining");
            Console.WriteLine("     Start: start mining");
            Console.WriteLine("");
            Console.WriteLine("Run ' [command] --help' for more information about a command.");
        }

        static void Init(string[] args)
        {
            var miner = new Miner();

            try
            {
                /*
                PoolInfo poolinfo = GetPoolServerAddressAndPort();
                if(poolinfo == null)
                {
                    return;
                }
                miner.PoolServerAddress = poolinfo.PoolAddress;
                miner.PoolServerPort = poolinfo.Port;
                */
                miner.PoolServerAddress = args[5];
                miner.PoolServerPort = int.Parse(args[6]);
                miner.SerialNo = args[1];
                miner.MinerType = EnumMinerType.POS;
                miner.WalletAddress = args[2];
                miner.PlotFilePath = args[3];
                miner.Capacity = long.Parse(args[4]);
            }
            catch
            {
                Console.WriteLine("Useage: dotnet MinerTest.dll Init <PoolServerAddress> <PoolServerPort> <SerialNo> <WalletAddress> <PlotFilePath> <Capacity>");
                Console.WriteLine("");
                Console.WriteLine("Parameters:");
                Console.WriteLine("     PoolServerAddress: IP address of pool server");
                Console.WriteLine("     PoolServerPort: Tcp port of pool server");
                Console.WriteLine("     SerialNo: The Serial No of POS");
                Console.WriteLine("     WalletAddress: Miner's wallet address");
                Console.WriteLine("     PlotFilePath: The directory path used to storage plot files");
                Console.WriteLine("     Capacity: Max storage capacity used to storage plot files");
                Console.WriteLine("");
                return;
            }

            miner.InitPlotFiles();
            miner.SaveSettings();
        }

        public static void StartMining(Miner oldMiner = null)
        {
            try
            {
                if (oldMiner != null)
                {
                    oldMiner.Dispose();
                    oldMiner = null;
                }
                var miner = Miner.LoadFromSetting();
                miner.Init();
                miner.Start();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                Task.Delay(3000).Wait();
                StartMining();
            }
        }

        static void Stop(string[] args)
        {

        }

        /// <summary>
        /// 调接口获取PoolAddress和PoolPort
        /// </summary>
        /// <returns></returns>
        static PoolInfo GetPoolServerAddressAndPort()
        {
            try
            {
                //调Api接口
                string url = $"https://poolapi.fiii.io/Api/NewMiners/GetSuitablePoolInfo";
                List<KeyValuePair<string, string>> dic = new List<KeyValuePair<string, string>>();
                ApiResponse response = ApiHelper.GetApi(url, dic);
                if (!response.HasError)
                {
                    PoolInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<PoolInfo>(response.Result.ToString());
                    return info;
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            return null;
        }
    }
}
