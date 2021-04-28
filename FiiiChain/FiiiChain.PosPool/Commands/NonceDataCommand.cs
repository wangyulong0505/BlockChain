// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using FiiiChain.PosPool.Apis;
using FiiiChain.PosPool.Sockets;
using FiiiChain.ShareModels.Msgs;
using System;
using System.Linq;

namespace FiiiChain.PosPool.Commands
{
    internal static class NonceDataCommand
    {
        internal static void Receive(TcpState e, PoolCommand cmd)
        {
            var msg = new NonceDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = PoolCache.WorkingMiners.FirstOrDefault(m => m.ClientAddress == e.Address);

            if (miner == null)
            {
                RejectCommand.Send(e);
                return;
            }

            var data = POC.CalculateScoopData(miner.WalletAddress, msg.MaxNonce, miner.CheckScoopNumber);

            if (Base16.Encode(data) == Base16.Encode(msg.ScoopData))
            {
                miner.IsConnected = true;
                miner.ConnectedTime = Time.EpochTime;
                miner.LatestHeartbeatTime = Time.EpochTime;
                if (PoolCache.NonceData.ContainsKey(miner.WalletAddress))
                {
                    PoolCache.NonceData[miner.WalletAddress] = msg.MaxNonce;
                    return;
                }
                else
                {
                    PoolCache.NonceData.Add(miner.WalletAddress, msg.MaxNonce);
                }
            }
        }

        internal static void Send(TcpState e, int scoopNumber)
        {
            var msg = new MaxNonceMsg();
            msg.RandomScoopNumber = scoopNumber;
            var cmd = PoolCommand.CreateCommand(CommandNames.MaxNonce, msg);
            PoolJob.TcpServer.SendCommand(e, cmd);
            LogHelper.Info("Send MaxNonceMsg To " + e.Address);
        }

        internal static void Send(TcpState e)
        {
            var msg = new MaxNonceMsg();
            msg.RandomScoopNumber = new Random().Next(0, 4095);
            var cmd = PoolCommand.CreateCommand(CommandNames.MaxNonce, msg);
            PoolJob.TcpServer.SendCommand(e, cmd);
        }

    }
}