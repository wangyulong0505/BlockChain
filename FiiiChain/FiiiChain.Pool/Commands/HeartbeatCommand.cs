﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.Pool.Sockets;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiiiChain.Pool.Commands
{
    internal static class HeartbeatCommand
    {
        internal static void Receive(TcpReceiveState e, PoolCommand cmd)
        {
            UpdateHeartTime(e);
        }

        internal static void UpdateHeartTime(TcpState e)
        {
            var miner = PoolCache.WorkingMiners.FirstOrDefault(x => x.ClientAddress == e.Address);
            if (miner != null)
            {
                miner.LatestHeartbeatTime = Time.EpochTime;
            }
        }
    }
}