// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.PosPool.Models
{
    internal class Miner
    {
        internal string WalletAddress { get; set; }
        internal string SerialNo { get; set; }
        internal string ClientAddress { get; set; }

        internal TcpClient Client { get; set; }
        internal NetworkStream Stream { get; set; }
        internal int CheckScoopNumber { get; set; }

        internal bool IsConnected { get; set; }
        internal long ConnectedTime { get; set; }
        internal long LatestHeartbeatTime { get; set; }
    }
}
