using FiiiChain.AliMQ;
using FiiiChain.Api.Test.Entities;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiiiChain.Api.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            MysqlOper oper = new MysqlOper();
            List<string> txids = new List<string>() { "1B25B3031A6F5227ED8A650F70045CCD67B309E24196A2CFEE860B8B7A7E39D4", "EF771498206FE2BE9BFF5C5F22943FDDB3F74370605E7E2584DA6C43FD15B8CF" };
            List<string> tableName = new List<string>() { "rewardlist20181126", "rewardlist20181127", "rewardlist20181128" };
            List<string> paidFilePaths = new List<string>() { "EF771498206FE2BE9BFF5C5F22943FDDB3F74370605E7E2584DA6C43FD15B8CF.txt", "1B25B3031A6F5227ED8A650F70045CCD67B309E24196A2CFEE860B8B7A7E39D4.txt" };
            List<string> unpaidFilePath = new List<string>() { "E0BB82B3C30F5C7AB220CEB638FD116D24A6E3100A431B9BCEF9208F69A8899F.txt" };
            await oper.GetMinersRewardByTxid(txids, tableName, paidFilePaths, unpaidFilePath, args[0]);
            
            ListPageUnspentTest utxo = new ListPageUnspentTest();
            await utxo.UTXOTest();
            */
            RabbitMQ mq = new RabbitMQ();
            mq.SendMessage();
            /*
            List<RewardSendMQExtend> list = mq.GetRewardSendMQs("Info_20190214.log");
            var sendList = list.GroupBy(q => q.TransactionHash);
            foreach (var item in sendList)
            {
                ProducerMessage<RewardSendMQ> producer = new ProducerMessage<RewardSendMQ>();
                producer.Initialize("ProductionReward", "PID_ProductionReward");
                producer.InitializeNormalProducer();
                //从配置文件中获取RewardSendMQ信息, 其实也可以从数据库获取信息
                List<RewardSendMQ> mqList = item.Select(q => new RewardSendMQ { Address = q.Address, Account = q.Account, Reward = q.Reward, SN = q.SN, CurrentDate = q.CurrentDate }).ToList();
                LogHelper.Info($"send aliyun message transactionhash = {item.Key}");
                producer.SendNormalMessage(mqList, item.Key);
            }
            */
            Console.ReadLine();
        }
    }
}
