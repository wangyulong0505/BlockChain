using FiiiChain.Framework;
using FiiiChain.NewPool.API;
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

namespace FiiiChain.NewPool
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
            var saveWorkTimer = new Timer(ReadConfig.PoolConfig.PoolSaveWorkTime);
            saveWorkTimer.Elapsed += PoolSaveWorkTimer_Elapsed;
            saveWorkTimer.Start();

            var heartTimer = new Timer(ReadConfig.PoolConfig.PoolHeartTime);
            heartTimer.Elapsed += HeartTimer_Elapsed;
            heartTimer.Start();
        }

        /// <summary>
        /// 心跳包定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                PoolHeartMsg msg = new PoolHeartMsg { HeartTime = Time.EpochTime, PoolId = ReadConfig.PoolConfig.PoolId };
                var json = JsonConvert.SerializeObject(msg);

                //给PoolCenter发送心跳包
                RabbitMqClient.Current.ProduceMessage(RabbitMqName.HeartPool, MsgType.HeartPool, json);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error on HeartTimer_Elapsed", ex);
            }
        }

        /// <summary>
        /// 更新Pool数据定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PoolSaveWorkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SavePoolInfoToRedis();
            SavePoolWorkingInfoToRedis();
        }

        private long uploadHash = 0;
        const int LogoutTime = 1000 * 60;

        /// <summary>
        /// 把Pool工作数据保存在redis中
        /// </summary>
        public void SavePoolWorkingInfoToRedis()
        {
            try
            {
                if (PoolCache.CurrentTask == null)
                    return;

                var totalEffort = PoolCache.CurrentTask.MinerEfforts.Sum(x => x.Effort);
                if (PoolCache.Efforts.ContainsKey(PoolCache.CurrentTask.CurrentBlockHeight))
                {
                    totalEffort += Convert.ToInt32(PoolCache.Efforts[PoolCache.CurrentTask.CurrentBlockHeight].Sum(x => x.Effort));
                }

                PoolWorkingInfo poolInfo = new PoolWorkingInfo
                {
                    HashRates = totalEffort - uploadHash,
                    Miners = PoolJob.m_socket.m_clients.Where(m=>m.Authorized && m.Subscribed).Select(x => x.WalletAddress).ToArray(),
                    PushTime = Time.EpochTime,
                };
                uploadHash = totalEffort;
                RedisApi.SavePoolWorkInfo(poolInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 把Pool信息保存redis
        /// </summary>
        public void SavePoolInfoToRedis()
        {
            try
            {
                uploadHash = 0;
                PoolInfo poolInfo = new PoolInfo
                {
                    MinerCount = PoolJob.m_socket.m_clients.Where(m => m.Authorized && m.Subscribed).Count(),
                    Port = GlobalParameters.IsTestnet? ReadConfig.SocketConfig.TestPort: ReadConfig.SocketConfig.TestPort,
                    PullTime = Time.EpochTime,
                    PoolId = ReadConfig.PoolConfig.PoolId,
                    PoolAddress = ReadConfig.SocketConfig.IP
                };
                RedisApi.SavePoolInfo(poolInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
        
        /// <summary>
        /// 保存工作量到redis
        /// </summary>
        /// <param name="height"></param>
        public void SaveMinerEffortToRedis(int height)
        {
            try
            {
                if (PoolCache.Efforts.ContainsKey(height))
                {
                    LogHelper.Debug("total reward = " + PoolCache.Efforts[height].Count);
                    List<EffortInfo> infos = PoolCache.Efforts[height];
                    if (infos == null)
                        infos = new List<EffortInfo>();

                    //infos.ForEach(x => x.Effort = 131072); //不能再默认值

                    RedisApi.SaveMinerEffort(height,infos);
                }
                else
                {
                    LogHelper.Debug("Efforts = NULL");
                    List<EffortInfo> infos = new List<EffortInfo>();
                    RedisApi.SaveMinerEffort(height, infos);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
        
    }
}
