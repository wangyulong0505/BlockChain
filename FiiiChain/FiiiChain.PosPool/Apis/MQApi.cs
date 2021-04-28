// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.KafkaMQ;
using FiiiChain.Messages;
using FiiiChain.PosPool.Commands;
using FiiiChain.PosPool.Models;
using FiiiChain.Pool.Redis;
using FiiiChain.PosPool.Sockets;
using FiiiChain.PoolMessages;
using FiiiChain.ShareModels;
using FiiiChain.ShareModels.Models;
using FiiiChain.ShareModels.Msgs;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace FiiiChain.PosPool.Apis
{
    public class MQApi
    {
        public static void SendForgeBlock(string account, long nonce, string startMsgId)
        {
            ForgeMsg forgeMsg = new ForgeMsg();
            forgeMsg.Account = account;
            forgeMsg.Nonce  = nonce;
            forgeMsg.StartMsgId = startMsgId;
            var json = JsonConvert.SerializeObject(forgeMsg);
            MqManager.Current.Send(MsgType.ForgeBlock, json);
        }

        public static MQApi Current;

        public MQApi()
        {
            try
            {
                MqManager.Current.Regist(MsgType.StartMining, ReceiveStartMsg);
                MqManager.Current.Regist(MsgType.StopMining, ReceiveStopMsg);
                MqManager.Current.Regist(MsgType.Login, ReceiveLoginMsg);
                MqManager.Current.Listen();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        public void ReceiveStartMsg(string json)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<StartMiningMsg>(json);

                if (msg == null)
                    return;

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
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return;
            }
        }

        public void ReceiveStopMsg(string json)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<StopMiningMsg>(json);

                LogHelper.Info("Receive StopMsg");

                if (msg == null || PoolCache.CurrentTask == null)
                    return;
                LogHelper.Info("Step 1");
                var stopTask = PoolCache.CurrentTask;

                if (msg.StopReason == StopReason.MiningSucesses)
                {
                    LogHelper.Info("Step 2");
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

        public void ReceiveLoginMsg(string json)
        {
            var msg = JsonConvert.DeserializeObject<MinerLoginMsg>(json);

            if (msg == null)
                return;

            LogHelper.Info("Receive LoginMsg");

            if (msg.ServerId == Setting.PoolId)
                return;

            Miner miner = PoolCache.WorkingMiners.FirstOrDefault(x=>x.WalletAddress == msg.Account && x.SerialNo == msg.SN);

            if (miner == null)
                return;

            if (PoolCache.NonceData.ContainsKey(miner.WalletAddress))
            {
                PoolCache.NonceData.Remove(miner.WalletAddress);
            }

            RejectCommand.Send(new TcpState { Client = miner.Client, Stream = miner.Stream, Address = miner.ClientAddress });
            //PoolCache.WorkingMiners.Remove(miner);
        }

        public void SendLoginMsg(MinerLoginMsg msg)
        {
            var json = JsonConvert.SerializeObject(msg);
            MqManager.Current.Send(MsgType.Login, json);
        }
    }
}
