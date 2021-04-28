using FiiiChain.AliMQ;
using FiiiChain.Api.Test.Entities;
using FiiiChain.Framework;
using FiiiChain.MiningPool.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiiiChain.Api.Test
{
    /// <summary>
    /// 小程序用来处理阿里云发送的消息
    /// </summary>
    public class RocketMQ
    {
        bool testnet = false;
        public const string CacheConnectionString = "server=rm-3nsnh13p454nv0r68.mysql.rds.aliyuncs.com;port=3306;user=fiiipool_dbo;password=8&KujS!sCw3Ex26f;database=fiiipool_main;SslMode=none;ConnectionTimeout=1200";
        public void SendAliReward()
        {
            ProducerMessage<RewardSendMQ> producer = new ProducerMessage<RewardSendMQ>();
            producer.Initialize((testnet ? "FiiiPayReward" : "ProductionReward"), (testnet ? "PID_FiiiPayReward" : "PID_ProductionReward"));
            producer.InitializeNormalProducer();
            //从配置文件中获取RewardSendMQ信息, 其实也可以从数据库获取信息
            List<RewardSendMQ> mqList = GetAddressAndAmountFromFiles();
            LogHelper.Info($"++++++++++++++++++++++++Start to Send++++++++++++++++++++++++++++");
            producer.SendNormalMessage(mqList, "");
        }

        /// <summary>
        /// 从文件中获取RewardSendMQ信息
        /// </summary>
        /// <returns></returns>
        private List<RewardSendMQ> GetAddressAndAmountFromFiles()
        {
            string filePath = "20190205.txt";
            List<RewardSendMQ> list = new List<RewardSendMQ>();
            //文件中读取address和amount
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line = null;
                    LogHelper.Info($"========begin to read data========");
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                    {
                        string address = line.Split(',')[0];
                        Miners miners = GetMinersInfoByAddress(address);
                        if (miners != null)
                        {
                            long amount = Convert.ToInt64(line.Split(',')[1]);
                            if (list.Exists(p => p.Address == address))
                            {
                                long existAmount = list.Single(a => a.Address == address).Reward;
                                list.Single(a => a.Address == address).Reward = existAmount + amount;
                                LogHelper.Info($"exist address send aliyun message Address = {address}, Reward = {existAmount + amount}, SN = {miners.SN}, CurrentDate = {"2019-02-05 01:00:00"}");
                            }
                            else
                            {
                                list.Add(new RewardSendMQ { Address = address, Reward = amount, Account = miners.Account, SN = miners.SN, CurrentDate = Time.GetEpochTime(2019, 2, 5, 1, 0, 0) });
                                LogHelper.Info($"send aliyun message Address = {address}, Reward = {amount}, SN = {miners.SN}, CurrentDate = {"2019-02-14 01:00:00"}");
                            }
                        }
                    }
                }
            }
            return list;
        }

        private Miners GetMinersInfoByAddress(string address)
        {
            string sql = $"SELECT Address, Account, SN FROM Miners WHERE Address = '{address}';";
            Miners miner = null;
            using (MySqlConnection conn = new MySqlConnection(CacheConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Connection.Open();
                    cmd.CommandTimeout = 1200;
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr.HasRows)
                            {
                                miner = new Miners();
                                miner.Address = dr.GetString(0);
                                miner.Account = dr.GetString(1);
                                miner.SN = dr.GetString(2);
                            }
                        }
                    }
                }
            }
            return miner;
        }

        //读取14号和5号的日志
        /* 1、先从日志文件中获取对应的TransactionHash
         * 2、根据TransactionHash获取对应的Address和Amount等信息
         * 3、组装阿里云消息队列发送消息
         */
        public List<RewardSendMQExtend> GetRewardSendMQs(string filePath)
        {
            HashSet<string> transactionHashes = new HashSet<string>();
            List<RewardSendMQExtend> list = new List<RewardSendMQExtend>();
            //1、先从文件中获取所有TransactionHash
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line = null;
                    LogHelper.Info($"========begin to read data========");
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                    {
                        if(line.Contains("this transaction hash is"))
                        {
                            transactionHashes.Add(line.Split(':')[3].Trim());
                        }
                    }
                }
            }
            //2、根据TransactionHash获取对应的RewardSendMQ信息
            foreach(string hash in transactionHashes)
            {
                RewardSendMQExtend rewardSend1 = null;
                RewardSendMQExtend rewardSend2 = null;
                string sql = $"SELECT m.Address, m.Account, m.SN, SUM(r.ActualReward), r.TransactionHash FROM rewardlist20190212 r LEFT JOIN Miners m ON m.Address = r.MinerAddress WHERE TransactionHash = '{hash}' GROUP BY r.MinerAddress;";
                using (MySqlConnection conn = new MySqlConnection(CacheConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Connection.Open();
                        cmd.CommandTimeout = 1200;
                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                rewardSend1 = new RewardSendMQExtend();
                                rewardSend1.Address = dr.GetString(0);
                                rewardSend1.Account = dr.GetString(1);
                                rewardSend1.SN = dr.GetString(2);
                                rewardSend1.Reward = dr.GetInt64(3);
                                rewardSend1.TransactionHash = dr.GetString(4);

                                list.Add(rewardSend1);
                            }
                        }
                    }
                }
                string sql2 = $"SELECT m.Address, m.Account, m.SN, SUM(r.ActualReward), r.TransactionHash FROM rewardlist20190213 r LEFT JOIN Miners m ON m.Address = r.MinerAddress WHERE TransactionHash = '{hash}' GROUP BY r.MinerAddress;";
                using (MySqlConnection conn = new MySqlConnection(CacheConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql2, conn))
                    {
                        cmd.Connection.Open();
                        cmd.CommandTimeout = 1200;
                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                rewardSend2 = new RewardSendMQExtend();
                                rewardSend2.Address = dr.GetString(0);
                                rewardSend2.Account = dr.GetString(1);
                                rewardSend2.SN = dr.GetString(2);
                                rewardSend2.Reward = dr.GetInt64(3);
                                rewardSend2.TransactionHash = dr.GetString(4);

                                list.Add(rewardSend2);
                            }
                        }
                    }
                }
            }
            //3、合并相同地址
            return list.GroupBy(q => new { q.Address, q.Account, q.SN }).Select(p => new RewardSendMQExtend { Address = p.Key.Address, Account = p.Key.Account, SN = p.Key.SN, Reward = p.Sum(a => a.Reward), TransactionHash = p.Select(a=>a.TransactionHash).First(), CurrentDate = Time.EpochTime }).ToList();
        }
    }
}
