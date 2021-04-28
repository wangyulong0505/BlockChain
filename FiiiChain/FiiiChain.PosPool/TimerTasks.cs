// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.KafkaMQ;
using FiiiChain.MiningPool.Shares;
using FiiiChain.Pool.Redis;
using FiiiChain.RabbitMQ;
using FiiiChain.ShareModels;
using FiiiChain.ShareModels.Models;
using FiiiChain.ShareModels.Msgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Timers;

namespace FiiiChain.PosPool
{
    public class TimerTasks
    {
        private static TimerTasks _current;
        public static TimerTasks Current
        {
            get
            {
                if (_current == null)
                    _current = new TimerTasks();
                return _current;
            }
        }

        public void Init()
        {
            const int updatePoolsTime = 1000 * 10;//10秒
            var updatePoolsTimer = new Timer(updatePoolsTime);
            updatePoolsTimer.AutoReset = true;
            updatePoolsTimer.Elapsed += UpdatePoolsTimer_Elapsed;
            updatePoolsTimer.Start();

            var heartTimer = new Timer(Setting.HEART_TIME);
            heartTimer.Elapsed += HeartTimer_Elapsed;
            heartTimer.Start();
        }

        private void HeartTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                PoolHeartMsg msg = new PoolHeartMsg { HeartTime = Time.EpochTime, PoolId = Setting.PoolId };
                var json = JsonConvert.SerializeObject(msg);
                //MqManager.Current.Send(MsgType.HeartPool, json);
                RabbitMqClient.Current.ProduceMessage(RabbitMqName.HeartPool, MsgType.HeartPool, json);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error on HeartTimer_Elapsed", ex);
            }
        }

        private void UpdatePoolsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            try
            {
                timer.Stop();
                SavePoolInfoToRedis();
                SavePoolWorkingInfoToRedis();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                timer.Start();
            }
        }

        private long uploadHash = 0;
        const int LogoutTime = 1000 * 60;

        public void SavePoolWorkingInfoToRedis()
        {
            try
            {
                var key = KeyHelper.GetPoolWorkingInfoKey(Setting.PoolId);

                if (PoolCache.CurrentTask == null)
                    return;

                var totalEffort = PoolCache.NonceData.Sum(x => x.Value);

                PoolWorkingInfo poolInfo = new PoolWorkingInfo
                {
                    HashRates = totalEffort - uploadHash,
                    Miners = PoolCache.WorkingMiners.Select(x => x.WalletAddress).ToArray(),
                    PushTime = Time.EpochTime,
                };
                uploadHash = totalEffort;
                RedisManager.Current.SaveDataToRedis(key, poolInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        public void SavePoolInfoToRedis()
        {
            try
            {

                uploadHash = 0;
                var key = KeyHelper.GetPoolInfoKey(Setting.PoolId);
                PoolInfo poolInfo = new PoolInfo
                {
                    MinerCount = PoolCache.WorkingMiners.Count(),
                    Port = Setting.PoolPort,
                    PullTime = Time.EpochTime,
                    PoolId = Setting.PoolId,
                    PoolAddress = Setting.PoolAddress
                };

                RedisManager.Current.SaveDataToRedis(key, poolInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
        
        public void SaveMinerEffortToRedis(int height)
        {
            try
            {
                LogHelper.Info("Step 3");
                var key = KeyHelper.GetMinerEffortKey(Setting.PoolId, height);
                LogHelper.Info("Step 4");
                if (PoolCache.NonceData != null && PoolCache.NonceData.Count > 0)
                {
                    //const int MaxTime = 1000 * 60 * 10;
                    //var address = PoolCache.WorkingMiners.Where(x => Time.EpochTime - x.LatestHeartbeatTime < MaxTime).Select(x => x.WalletAddress);
                    var address = PoolCache.WorkingMiners.Select(x => x.WalletAddress);
                    var infos = PoolCache.NonceData.Where(x => address.Contains(x.Key)).Select(x => new EffortInfo { Account = x.Key, Effort = x.Value, BlockHeight = height }).ToList();
                    LogHelper.Debug($"TotleCount = '{infos.Count} 'total reward = " + infos.Sum(x => x.Effort));
                    RedisManager.Current.SaveDataToRedis(key, infos);
                }
                else
                {
                    LogHelper.Debug("Efforts = NULL");
                    List<EffortInfo> infos = new List<EffortInfo>();
                    RedisManager.Current.SaveDataToRedis(key, infos);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
    }
}
