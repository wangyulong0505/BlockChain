using FiiiChain.NewPool.API;
using FiiiChain.PoolMessages;
using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using ServerSocket.Stratum.ParamsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool.PoolCmd
{
    public class SubscribeCmd : BaseCmd
    {

        public static void Receive(ClientUser client, object[] paramList)
        {
            if (!client.Authorized)
            {//未授权
                AuthorizeCmd.Reply(client,false, ResultErrorCode.UNAUTHORIZED_ERROR);
                return;
            }
            client.Subscribed = true; //已订阅
            Reply(client, RedisApi.GetMinerDiffculty(client.WalletAddress)); //下发难度值

            if (PoolCache.CurrentTask != null)
            {//重新任务下发
                NotifyCmd.Send(client, PoolCache.CurrentTask.CurrentStartMsg, false);
            }

        }

        public static void Reply(ClientUser client, double result, int errorCode = 0)
        {
            Reply(client, MethodId.Subscribe, new object[] { result }, errorCode);
        }

    }
}
