using FiiiChain.Framework;
using FiiiChain.MiningPool.Shares;
using FiiiChain.Pool.Redis;
using FiiiChain.ShareModels.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool.API
{
    public class RedisApi
    {
        private static string PoolMinerDiffcultyKey = "Pool:Diffculty:{0}";

        #region 获取矿工难度值
        public static double GetMinerDiffculty(string walletAddress)
        {
            string Key = string.Format(PoolMinerDiffcultyKey, walletAddress);
            double diffculty = RedisManager.Current.GetDataInRedis<double>(Key);

            return diffculty>0?ReadConfig.PoolConfig.DefaultDiffculty: ReadConfig.PoolConfig.DefaultDiffculty;
        }

        public static bool SaverDiffculty(string walletAddress,double diffculty)
        {
            string Key = string.Format(PoolMinerDiffcultyKey,walletAddress);
            return RedisManager.Current.SaveDataToRedis(Key, diffculty);
        }
        #endregion

        public static bool SavePoolWorkInfo(PoolWorkingInfo data)
        {
            //{PoolId}_WORKING_TEST/{PoolId}_WORKING_MAIN
            string Key = KeyHelper.GetPoolWorkingInfoKey(ReadConfig.PoolConfig.PoolId);
            return RedisManager.Current.SaveDataToRedis(Key, data);
        }

        /// <summary>
        /// 
        /// MiningPool.API读取Redis出来用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SavePoolInfo(PoolInfo data)
        {
            //{PoolId}_TEST/{PoolId}_MAIN
            string Key = KeyHelper.GetPoolInfoKey(ReadConfig.PoolConfig.PoolId);
            return RedisManager.Current.SaveDataToRedis(Key, data);
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="height"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SaveMinerEffort(int height, List<EffortInfo> data)
        {
            //{PoolId}_TEST_EFFORT_{height}
            string key = KeyHelper.GetMinerEffortKey(ReadConfig.PoolConfig.PoolId, height);
            return RedisManager.Current.SaveDataToRedis(key, data);
        }

        //public static bool UpdateClientCount(string key,int count)
        //{
        //   return RedisManager.Current.SaveDataToRedis(PoolSocketKey, count);
        //}
    }
}
