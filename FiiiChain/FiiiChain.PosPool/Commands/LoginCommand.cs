// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.PosPool.Apis;
using FiiiChain.PosPool.Models;
using FiiiChain.PosPool.Sockets;
using FiiiChain.PoolMessages;
using FiiiChain.ShareModels.Msgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.PosPool.Commands
{
    internal static class LoginCommand
    {

        internal static void Receive(TcpReceiveState e, PoolCommand cmd)
        {
            var loginMsg = new LoginMsg();
            int index = 0;
            loginMsg.Deserialize(cmd.Payload, ref index);

            if (!MinerApi.ValidateMiner(loginMsg.WalletAddress, loginMsg.SerialNo))
            {
                RejectCommand.Send(e);
                return;
            }

            //TODO: address and SerialNo and account only for one Minner
            var miner = PoolCache.WorkingMiners.FirstOrDefault(m => m.WalletAddress == loginMsg.WalletAddress || m.ClientAddress == e.Address || m.SerialNo == loginMsg.SerialNo);

            if (miner != null)
            {
                try
                {
                    //miner.Client.Client.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    PoolCache.WorkingMiners.Remove(miner);
                }
                catch
                {

                }
            }

            miner = new Miner();
            miner.SerialNo = loginMsg.SerialNo;
            miner.WalletAddress = loginMsg.WalletAddress;
            miner.ClientAddress = e.Address;
            miner.Client = e.Client;
            miner.Stream = e.Stream;

            LoginCommand.SendLoginResult(e, true);
            LogHelper.Info(miner.ClientAddress + " login success");

            if (PoolCache.NonceData.ContainsKey(miner.WalletAddress))
            {
                PoolCache.NonceData[miner.WalletAddress] = 100;
            }
            else
            {
                PoolCache.NonceData.Add(miner.WalletAddress, 100);
            }

            Random random = new Random();
            miner.CheckScoopNumber = random.Next(0, POC.MAX_SCOOP_NUMBER + 1);
            PoolCache.WorkingMiners.Add(miner);
            LogHelper.Info("Current Miners: " + PoolCache.WorkingMiners.Count);
            LogHelper.Info(miner.ClientAddress + "==" + PoolCache.WorkingMiners.Last().ClientAddress);
            NonceDataCommand.Send(e, miner.CheckScoopNumber);
        }

        internal static void SendLoginResult(TcpState e, bool result)
        {
            if (PoolJob.TcpServer != null)
            {
                var msg = new LoginResultMsg();
                msg.Result = result;
                var cmd = PoolCommand.CreateCommand(CommandNames.LoginResult, msg);
                PoolJob.TcpServer.SendCommand(e, cmd);
            }
        }
    }
}
