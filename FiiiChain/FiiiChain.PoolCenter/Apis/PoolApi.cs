// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.Messages;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using FiiiChain.MiningPool.Shares;
using FiiiChain.Pool.Redis;
using FiiiChain.ShareModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiiiChain.PoolCenter.Apis
{
    public class PoolApi
    {
        public static void SaveHashRates()
        {
            //var poolkeys = CenterCache.Pools.Where(x => Time.EpochTime - x.Value < Setting.MAX_HEART_TIME).Select(x => KeyHelper.GetPoolWorkingInfoKey(x.Key));
            //var poolInfos = poolkeys.Select(key => RedisManager.Current.GetDataInRedis<PoolWorkingInfo>(key));
            //HashRatesComponent hashRatesComponent = new HashRatesComponent();
            //var hashCount = poolInfos.Where(x => Time.EpochTime - x.PushTime < 10000).Sum(x => x.HashRates);
            //hashRatesComponent.SaveHashRates(hashCount);
        }

        public static void SaveBlockRates()
        {
            long difficulty = 0;
            if (PoolCenterJob.Current.CurrentStartMiningMsg != null)
            {
                var block = RedisManager.Current.GetDataInRedis<BlockMsg>(PoolCenterJob.Current.CurrentStartMiningMsg.Id);
                difficulty = Convert.ToInt64(POC.CalculateDifficulty(block.Header.Bits));
            }
            else
            {
                var height = NodeApi.Current.GetBlockHeight();
                var block = NodeApi.Current.GetBlockByHeight(height);
                difficulty = Convert.ToInt64(POC.CalculateDifficulty(block.Header.Bits));
            }
            
            BlockRatesComponent component = new BlockRatesComponent();
            component.SaveBlockRates(CenterCache.GenarateBlockCount, difficulty);
            CenterCache.GenarateBlockCount = 0;
        }

        //public static void SaveRewards(string startMsgId, long nonce, int height)
        //{
        //    try
        //    {
        //        long totalEffort = 0;
        //        List<EffortInfo> effortInfos = new List<EffortInfo>();
        //        try
        //        {
        //            var activePools = CenterCache.Pools.Where(x => Time.EpochTime - x.Value < Setting.MAX_HEART_TIME);
        //            LogHelper.Info("SaveRewards ActivePools Count = " + activePools.Count());
        //            var effortkeys = activePools.Select(x => KeyHelper.GetMinerEffortKey(x.Key, height));

        //            effortkeys.ToList().ForEach(key => {
        //                var infos = RedisManager.Current.GetDataInRedis<List<EffortInfo>>(key);
        //                if (infos != null)
        //                {
        //                    effortInfos.AddRange(infos);
        //                }
        //                else
        //                {
        //                    LogHelper.Info(string.Format("{0} = NULL", key));
        //                }
        //            });

        //            effortInfos.RemoveAll(x => x == null || x.Effort == 0);

        //            totalEffort = effortInfos.Sum(x => x.Effort);
        //            LogHelper.Info("totalEffort = " + totalEffort);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.Error(ex.ToString());
        //        }

        //        var rewardAddresses = effortInfos.Select(x => x.Account).Distinct().ToArray();

        //        var block = RedisManager.Current.GetDataInRedis<BlockMsg>(startMsgId);
        //        if (block == null || block.Header.Height == 0)
        //            return;

        //        var totalReward = block.Transactions[0].Outputs[0].Amount;

        //        Blocks blocks = new Blocks();
        //        blocks.Generator = block.Header.GeneratorId;
        //        blocks.Hash = block.Header.GetHash();
        //        blocks.Height = block.Header.Height;
        //        blocks.Nonce = nonce;
        //        blocks.TotalHash = totalEffort;
        //        blocks.TotalReward = totalReward;

        //        var rewards = new List<RewardList>();

        //        foreach (var minerAddress in rewardAddresses)
        //        {
        //            RewardList rewardList = new RewardList();
        //            rewardList.BlockHash = blocks.Hash;
        //            rewardList.GenerateTime = blocks.Timstamp;
        //            rewardList.MinerAddress = minerAddress;
        //            var hashCount = effortInfos.FirstOrDefault(x => x.Account == minerAddress).Effort;
        //            rewardList.Hashes = hashCount;
        //            rewardList.OriginalReward = (long)Math.Floor(totalReward * ((double)hashCount / (double)totalEffort));
        //            rewardList.TransactionHash = "";
        //            rewards.Add(rewardList);
        //        }

        //        var blockComponent = new BlocksComponent();
        //        blockComponent.SaveBlockAndRewardLists(blocks, rewards);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(ex.ToString());
        //    }
        //}


        public static List<Miners> GetAllMiners()
        {
            MinersComponent component = new MinersComponent();
            var miners = component.GetAllMiners().Where(x => x.Status == 0).ToList();
            return miners;
        }

        public static void SaveRewards(string startMsgId, long nonce, int height)
        {
            try
            {
                var block = RedisManager.Current.GetDataInRedis<BlockMsg>(startMsgId);
                if (block == null || block.Header.Height == 0)
                {
                    return;
                }

                var miners = GetAllMiners();
                var rewardAddresses = miners.Select(x => x.Address);


                Blocks blocks = new Blocks();
                blocks.Generator = block.Header.GeneratorId;
                blocks.Hash = block.Header.GetHash();
                blocks.Height = block.Header.Height;
                blocks.Nonce = nonce;

                var baseReward = POC.GetNewBlockReward(block.Header.Height);
                var totalReward = block.Transactions[0].Outputs[0].Amount;

                if (totalReward < baseReward)
                {
                    totalReward = baseReward;
                }

                var rewards = new List<RewardList>();
                long totalEffort = 0;
                var minerCount = 0;
                foreach (var address in rewardAddresses)
                {
                    var key = "Pool:MinerMaxNonce:" + address;
                    var maxNonce = RedisManager.Current.GetDataInRedis<string>(key);
                    if (string.IsNullOrEmpty(maxNonce))
                    {
                        continue;
                    }
                    var effort = int.Parse(maxNonce);
                    RewardList rewardList = new RewardList();
                    rewardList.BlockHash = blocks.Hash;
                    rewardList.GenerateTime = blocks.Timstamp;
                    rewardList.MinerAddress = address;
                    rewardList.Hashes = effort;
                    //rewardList.OriginalReward = (long)Math.Floor(totalReward * ((double)hashCount / (double)totalEffort));
                    rewardList.TransactionHash = "";
                    rewards.Add(rewardList);
                    totalEffort += effort;
                    minerCount++;
                }
                var avgTotalRewards = totalReward * 0.8;
                var leftReward = totalReward - avgTotalRewards;
                long avgReward = 0;
                if (minerCount > 0)
                {
                    avgReward = (long)avgTotalRewards / minerCount;
                }
                rewards.ForEach(x => x.OriginalReward = avgReward + (long)Math.Floor(leftReward * ((double)x.Hashes / (double)totalEffort)));

                //rewards.ForEach(x => x.OriginalReward = (long)Math.Floor(totalReward * ((double)x.Hashes / (double)totalEffort)));

                blocks.TotalHash = totalEffort;
                blocks.TotalReward = totalReward;

                var blockComponent = new BlocksComponent();
                blockComponent.SaveBlockAndRewardLists(blocks, rewards);
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        public static void SaveRewards_New(string startMsgId, long nonce, int height)
        {
            try
            {
                var block = RedisManager.Current.GetDataInRedis<BlockMsg>(startMsgId);
                if (block == null || block.Header.Height == 0)
                {
                    return;
                }

                //获取所有Pool的所有参与这个区块挖矿的Miner
                List<EffortInfo> list = GetMinerShares(height);
                if (list.Count == 0)
                    return;

                var effortList = list.GroupBy(x => x.Account).Select(g => new
                {
                    Account = g.Key,
                    Height = g.First().BlockHeight,
                    Effort = g.Sum(s => s.Effort)
                });
                

                var miners = GetAllMiners();
                var rewardAddresses = miners.Select(x => x.Address);


                Blocks blocks = new Blocks();
                blocks.Generator = block.Header.GeneratorId;
                blocks.Hash = block.Header.GetHash();
                blocks.Height = block.Header.Height;
                blocks.Nonce = nonce;

                var baseReward = POC.GetNewBlockReward(block.Header.Height);
                var totalReward = block.Transactions[0].Outputs[0].Amount;
                if (totalReward < baseReward)
                {
                    totalReward = baseReward;
                }

                var rewards = new List<RewardList>();
                long totalEffort = 0;
                var minerCount = 0;

                long totalOriginalEffor = effortList.Sum(a => a.Effort); //总Shares
                foreach (var effor in effortList)
                {
                    if (!rewardAddresses.Contains(effor.Account))
                        continue;

                    long minerReward = (long)(totalReward * (1 - 1.0 )*(effor.Effort / totalOriginalEffor)); //矿工奖励

                    RewardList rewardList = new RewardList();
                    rewardList.BlockHash = blocks.Hash;
                    rewardList.GenerateTime = blocks.Timstamp;
                    rewardList.MinerAddress = effor.Account;
                    rewardList.Hashes = 1000; //int.Parse(maxNonce)???
                    rewardList.OriginalReward = minerReward;
                    rewardList.TransactionHash = "";
                    rewards.Add(rewardList);

                    totalEffort += minerReward;
                    minerCount++;

                }
               
                blocks.TotalHash = totalEffort;
                blocks.TotalReward = totalReward;

                var blockComponent = new BlocksComponent();
                blockComponent.SaveBlockAndRewardLists(blocks, rewards);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        private static List<EffortInfo> GetMinerShares(int blockHeight)
        {
            List<EffortInfo> allList = new List<EffortInfo>();

            string key = KeyHelper.GetPoolCenterName(GlobalParameters.IsTestnet);
            List<string> poolIds = RedisManager.Current.GetDataInRedis<List<string>>(key);
            if (poolIds == null || !poolIds.Any())
                return allList;

            var poolInfoKeys = poolIds.Select(x => KeyHelper.GetPoolInfoKey(x));
            if (poolInfoKeys == null || !poolInfoKeys.Any())
                return allList;

            List<PoolInfo> serverList = poolInfoKeys.Select(x => RedisManager.Current.GetDataInRedis<PoolInfo>(x)).ToList();
            if (serverList == null || !serverList.Any())
                return allList;

            foreach (var poolInfo in serverList)
            {
                //{PoolId}_TEST_EFFORT_{height}
                string efforkey = KeyHelper.GetMinerEffortKey(poolInfo.PoolId, blockHeight);
                List<EffortInfo> effortList = RedisManager.Current.GetDataInRedis<List<EffortInfo>>(key);
                if (effortList != null)
                {
                    allList.AddRange(effortList);
                }
            }
            return allList;
        }

    }
}
