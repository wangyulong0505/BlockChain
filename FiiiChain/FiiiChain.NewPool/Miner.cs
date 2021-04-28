using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool
{
    internal class Miner
    {
        internal string WalletAddress { get; set; }
        internal string SerialNo { get; set; }
        internal string ClientAddress { get; set; }

        //internal TcpClient Client { get; set; }
       // internal NetworkStream Stream { get; set; }
        internal int CheckScoopNumber { get; set; }

        internal bool IsConnected { get; set; }
        internal long ConnectedTime { get; set; }
        internal long LatestHeartbeatTime { get; set; }
    }
}
