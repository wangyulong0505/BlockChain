using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.NewPool.API;
using FiiiChain.NewPool.Models;
using FiiiChain.PoolMessages;
using FiiiChain.ShareModels.Msgs;
using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using ServerSocket.Stratum.ParamsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.NewPool.PoolCmd
{
    public class SubmitCmd : BaseCmd
    {
        public static void Send()
        {

        }

        public static void Receive(ClientUser client, object[] paramList)
        {
            if (!client.Authorized)
            {//未授权
                Reply(client,false, ResultErrorCode.UNAUTHORIZED_ERROR);
                return;
            }
            if (!client.Subscribed)
            {//未订阅
                Reply(client,false, ResultErrorCode.UNSUBSCRIBER_ERROR);
                return;
            }
            SubmitModel model = StratumHelper.GetSubmitModel(paramList);
            if (model == null)
            {//参数error
                Reply(client, false, ResultErrorCode.PARAMS_ERROR);
                return;
            }

            if (PoolCache.CurrentTask == null || PoolCache.CurrentTask.State != MiningState.Mining)
            {
                Reply(client,false, ResultErrorCode.BLOCK_TASKNOTFOUND_ERROR);
                return;
            } 
            PoolCache.CurrentTask.LastReceiveTime = Time.EpochTime;

            if (model.BlockHeight != PoolCache.CurrentTask.CurrentBlockHeight)
            {//区块高度不符合要求
                LogHelper.Info($"Received invalid scoop data from {client.IPAddress}, nonce is{model.Nonce},model.BlockHeight");

                Reply(client, false, ResultErrorCode.BLOCK_HEIGHT_ERROR);

                Task.Delay(1000).Wait();
                if (PoolCache.CurrentTask != null)
                {//重新任务下发
                    NotifyCmd.Send(client, PoolCache.CurrentTask.CurrentStartMsg, false);
                }
                return;
            }
            LogHelper.Info($"Received scoop data from {client.IPAddress}, nonce is {model.Nonce}, scoop number is {model.ScoopNumber}, block height is {model.BlockHeight}");

            if (model.ScoopNumber != PoolCache.CurrentTask.CurrentScoopNumber)
            {
                LogHelper.Info($"Received invalid scoop data from {client.IPAddress}, nonce is {model.Nonce}, ScoopNumber is {PoolCache.CurrentTask.CurrentScoopNumber}/{model.ScoopNumber}");
                Reply(client, false, ResultErrorCode.BLOCK_SCOOPNUMBER_ERROR);
                return;
            }
            #region 验证难度值的是否符合要求【新增】
            double diffculty = RedisApi.GetMinerDiffculty(client.WalletAddress);
            ////
            #endregion


            MinerEffort minerinfo = PoolCache.CurrentTask.MinerEfforts.FirstOrDefault(x => x.Account == client.WalletAddress);
            if (minerinfo == null)
            {
                PoolCache.CurrentTask.MinerEfforts.Add(new MinerEffort { Account = client.WalletAddress, Effort = 1,Diffculty = diffculty,CurrentNonce = model.Nonce });
            }
            else
            {
                if (minerinfo.Effort == ReadConfig.PoolConfig.MaxNonceCount)
                {
                    Reply(client, false, ResultErrorCode.BLOCK_EFFORTMAXNONCE_ERROR);
                    return;
                }

                if (minerinfo.CurrentNonce >= model.Nonce)
                {//防重复提交
                    return;
                }

                minerinfo.CurrentNonce = model.Nonce; //记录已提交过的Nonce
                minerinfo.Effort++; //矿工添加算力
            }

            

            var verResult = POC.Verify(PoolCache.CurrentTask.BaseTarget, model.Target);

            LogHelper.Debug("Bits:" + POC.ConvertBitsToBigInt(PoolCache.CurrentTask.BaseTarget).ToString("X").PadLeft(64, '0'));
            LogHelper.Debug("Hash:" + Base16.Encode(model.Target));
            LogHelper.Debug("Verify Result is " + verResult);

            if (!verResult)
            {
                Reply(client, false, ResultErrorCode.BLOCK_VERIFY_ERROR);
                return;
            }
                
            ForgeMsg forgeMsg = new ForgeMsg();
            forgeMsg.Account = model.WalletAddress;
            forgeMsg.Nonce = model.Nonce;
            forgeMsg.StartMsgId = PoolCache.CurrentTask.Id;
            RabbitMQApi.SendForgeBlock(model.WalletAddress, model.Nonce, PoolCache.CurrentTask.Id);

        }

        public static void Reply(ClientUser client, bool result, int errorCode = 0)
        {
            Reply(client, MethodId.Submit, new object[] { result }, errorCode);
        }
    }
}
