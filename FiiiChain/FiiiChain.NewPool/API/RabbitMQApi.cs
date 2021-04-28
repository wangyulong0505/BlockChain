using FiiiChain.Framework;
using FiiiChain.Messages;
using FiiiChain.NewPool.PoolCmd;
using FiiiChain.Pool.Redis;
using FiiiChain.PoolMessages;
using FiiiChain.RabbitMQ;
using FiiiChain.ShareModels;
using FiiiChain.ShareModels.Models;
using FiiiChain.ShareModels.Msgs;
using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiiiChain.NewPool.API
{
    /// <summary>
    /// RabbitMQ消息队列接口
    /// </summary>
    public class RabbitMQApi
    {
        /// <summary>
        /// 接口实例
        /// </summary>
        public static RabbitMQApi Current;
        static SafeCollection<string> ForgeJsons = new SafeCollection<string>();
        /// <summary>
        /// 构造函数，RabbitMQ注册三种类型的消息，然后监听这三种类型的消息
        /// </summary>
        public RabbitMQApi()
        {
            try
            {
                //三种消息需要创建三个消息队列
                RabbitMqClient.Current.Regist(MsgType.StartMining, ReceiveStartMsg);
                RabbitMqClient.Current.Regist(MsgType.StopMining, ReceiveStopMsg);
                RabbitMqClient.Current.Regist(MsgType.Login, ReceiveLoginMsg);
                RabbitMqClient.Current.Listen();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"RabbitMQApi,error:{ex.Message}!");
            }
        }
        /// <summary>
        /// RabbitMQ发送ForgetMsg信息
        /// </summary>
        /// <param name="account"></param>
        /// <param name="nonce"></param>
        /// <param name="startMsgId"></param>
        public static void SendForgeBlock(string account, long nonce, string startMsgId)
        {
            ForgeMsg forgeMsg = new ForgeMsg();
            forgeMsg.Account = account;
            forgeMsg.Nonce = nonce;
            forgeMsg.StartMsgId = startMsgId;
            string json = JsonConvert.SerializeObject(forgeMsg);
            RabbitMqClient.Current.ProduceMessage(RabbitMqName.ForgetBlock, MsgType.ForgetBlock, json);
        }

        /// <summary>
        /// 接收开启命令
        /// 接收到的json反序列化StartMiningMsg对象，构造PoolTask类，添加到Pool任务中
        /// </summary>
        /// <param name="json"></param>
        public void ReceiveStartMsg(string json)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<StartMiningMsg>(json);
                if (msg == null) { return; }

                LogHelper.Info("Receive StartMsg");

                PoolTask poolTask = new PoolTask();
                poolTask.CurrentBlockHeight = msg.BlockHeight;
                poolTask.CurrentScoopNumber = msg.ScoopNumber;
                poolTask.CurrentStartMsg = msg.GetStartMsg();
                poolTask.GeneratingBlock = RedisManager.Current.GetDataInRedis<BlockMsg>(msg.Id.ToString());
                poolTask.BaseTarget = msg.BaseTarget;
                poolTask.StartTime = msg.StartTime;
                poolTask.State = MiningState.Wait;
                poolTask.Id = msg.Id;

                PoolCache.poolTasks.Clear();
                PoolCache.poolTasks.Add(poolTask);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 接收停止命令
        /// 接收到的json反序列化StopMiningMsg对象，构造StopMsg类，遍历所有的矿工发送TCP停止命令
        /// </summary>
        /// <param name="json"></param>
        public void ReceiveStopMsg(string json)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<StopMiningMsg>(json);

                LogHelper.Info("Receive StopMsg");

                if (msg == null || PoolCache.CurrentTask == null) return;

                foreach (ClientUser miner in PoolJob.m_socket.m_clients)
                {//回应矿工
                    try
                    {
                        if (msg.StopReason == StopReason.MiningSucesses)
                        {
                            //true=这个区块已经被挖到了
                            NotifyCmd.Send(miner, PoolCache.CurrentTask.CurrentStartMsg, true);
                        }
                        //回复miner，先停止提交
                        SubmitCmd.Reply(miner, false, ResultErrorCode.SUBMIT_STOP_ERROR);   
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.ToString());
                    }
                }

                var stopTask = PoolCache.CurrentTask;
                //计算每个账户的工作量
                if (PoolCache.Efforts.ContainsKey(stopTask.CurrentBlockHeight))
                {
                    var items = PoolCache.Efforts[stopTask.CurrentBlockHeight]; //当前区块高度的List<EffortInfo>

                    stopTask.MinerEfforts.ForEach(x =>
                    {
                        //计算shares
                        long shares = (long)x.Diffculty * x.Effort;

                        var item = items.FirstOrDefault(p => p.Account == x.Account);
                        if (item == null)
                        {
                            //items.Add(new EffortInfo { Account = x.Account, Effort = x.Effort, BlockHeight = stopTask.CurrentBlockHeight});
                            items.Add(new EffortInfo { Account = x.Account, Effort = shares, BlockHeight = stopTask.CurrentBlockHeight });
                        }
                        else
                        {
                            //item.Effort += x.Effort;
                            item.Effort += shares;
                        }
                    });
                }
                else
                {
                    var efforts = stopTask.MinerEfforts.Select(x => new EffortInfo { Account = x.Account, Effort = x.Effort, BlockHeight = stopTask.CurrentBlockHeight }).ToList();
                    PoolCache.Efforts.Add(stopTask.CurrentBlockHeight, efforts);
                }
                //成功挖到区块，工作量保存在redis中，清空以前区块的Task
                if (msg.StopReason == StopReason.MiningSucesses)
                {
                    TimerTasks.Current.SaveMinerEffortToRedis(msg.CurrentHeight);
                    PoolCache.poolTasks.RemoveAll(x => x.CurrentBlockHeight <= msg.CurrentHeight);
                }
                PoolCache.CurrentTask = null;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public void ReceiveLoginMsg(string json)
        {
            var msg = JsonConvert.DeserializeObject<MinerLoginMsg>(json);
            if (msg == null) return;

            LogHelper.Info("Receive LoginMsg");

            if (msg.ServerId == ReadConfig.PoolConfig.PoolId)
                return;

            var miner = PoolJob.m_socket.m_clients.FirstOrDefault(x => x.SerialNo == msg.SN || x.WalletAddress == msg.Account);
            if (miner == null)
                return;

            //重复授权错误
            AuthorizeCmd.Reply(miner,false, ResultErrorCode.AUTHORIZED_REPEAT_ERROR);
            //发现错误，断开连接
            PoolJob.m_socket.CloseClient(miner);
        }

        /// <summary>
        /// 发送登录消息到RabbitMq
        /// </summary>
        /// <param name="msg"></param>
        public void SendLoginMsg(MinerLoginMsg msg)
        {
            var json = JsonConvert.SerializeObject(msg);
            RabbitMqClient.Current.ProduceMessage(RabbitMqName.Login, MsgType.Login, json);
        }
    }
}
