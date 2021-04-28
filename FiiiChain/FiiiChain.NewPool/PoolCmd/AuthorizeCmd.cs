using FiiiChain.NewPool.API;
using FiiiChain.ShareModels.Msgs;
using FiiiChain.Stratum;
using FiiiChain.Stratum.ParamsModel;
using FiiiChain.Stratum.PoolSocket;
using System;
using System.Collections.Generic;

namespace FiiiChain.NewPool.PoolCmd
{
    public class AuthorizeCmd: BaseCmd
    {
        public static void Send()
        {
            
        }

        public static void Receive(ClientUser client, object[] paramList)
        {

            AuthorizeModel model = StratumHelper.GetAuthorizeModel(paramList);
            if (model == null)
            {
                Reply(client,false, ResultErrorCode.PARAMS_ERROR);
                return;
            }
            //验证矿工身份
            if (!MinerApi.ValidateMiner(model.WalletAddress, model.SerialNo))
            {
                Reply(client, false, ResultErrorCode.AUTHORIZED_ERROR);
                return;
            }
            List<ClientUser> clients = PoolJob.m_socket.m_clients.FindAll(m => m.WalletAddress == model.WalletAddress || m.SerialNo == model.SerialNo);
            foreach (var miner in clients)
            {//已存在的先干掉
                Reply(miner, false, ResultErrorCode.AUTHORIZED_REPEAT_ERROR);
                PoolJob.m_socket.CloseClient(miner);
                return;
            }
            client.WalletAddress = model.WalletAddress;
            client.SerialNo = model.SerialNo;
            client.ActiveDateTime = DateTime.Now;
            client.Authorized = true; //已授权
            client.Subscribed = false;
            //Random random = new Random();
            //client.CheckScoopNumber = random.Next(0, POC.MAX_SCOOP_NUMBER + 1);

            Reply(client, true); //回复client授权成功

            MinerLoginMsg loginMinerMsg = new MinerLoginMsg();
            loginMinerMsg.Account = model.WalletAddress;
            loginMinerMsg.SN = model.SerialNo;
            loginMinerMsg.ServerId = ReadConfig.PoolConfig.PoolId;
            RabbitMQApi.Current.SendLoginMsg(loginMinerMsg); //队列通知登陆

        }

        public static void Reply(ClientUser client, bool result, int errorCode = 0)
        {
            Reply(client, MethodId.Authorize, new object[] { result }, errorCode);
        }

    }
}
