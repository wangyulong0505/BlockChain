using FiiiChain.Framework;
using FiiiChain.KafkaMQ;
using FiiiChain.PosPool.Apis;
using FiiiChain.PosPool.Helpers;
using FiiiChain.PosPool.Models;
using FiiiChain.PosPool.Sockets;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace FiiiChain.PosPool
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
            //System.Threading.Tasks.Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (SocketServer._queue.Count > 0)
            //        {
            //            var d = SocketServer._queue.Dequeue();
            //            if (d != null)
            //                PoolJob.TcpServer.ReceivedCommandAction(d);
            //        }
            //        Task.Delay(300).Wait();
            //    }
            //});
            Console.WriteLine("Pool Start !!!");
        }

        static void Init()
        {
            /*
            string xmlFile = Path.Combine(Directory.GetCurrentDirectory(), "PoolConfig.xml");
            if (!File.Exists(xmlFile))
            {
                LogHelper.Error("Can Not found PoolConfig.xml");
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            XmlNode xns = xmlDoc.SelectSingleNode("PoolConfig/PoolAddress");
            if (xns == null || string.IsNullOrEmpty(xns.InnerText))
                return;
            XmlNode nos = xmlDoc.SelectSingleNode("PoolConfig/PoolKafkaGroupName");
            if (nos == null || string.IsNullOrEmpty(nos.InnerText))
                return;

            XmlNode tcpMaxCount = xmlDoc.SelectSingleNode("PoolConfig/TcpMaxCount");
            int maxCount = -1;
            if (tcpMaxCount != null && int.TryParse(tcpMaxCount.InnerText, out maxCount) && maxCount > 0)
            {
                Setting.Max_TCP_Count = maxCount;
            }

            KafkaInfo.MqName = nos.InnerText;
            Setting.PoolAddress = xns.InnerText.Trim();
            //LoginPool.Current = new LoginPool();
            //DataAnalysisPool.Current = new DataAnalysisPool();
            //DbHelper.Current = new DbHelper();
            //MsgPool.Current = new MsgPool();
            */
            RabbitMQApi.Current = new RabbitMQApi();
        }
    }
}
