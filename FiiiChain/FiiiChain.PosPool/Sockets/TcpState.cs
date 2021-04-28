// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System.Net.Sockets;

namespace FiiiChain.PosPool.Sockets
{
    internal class TcpState
    {
        internal TcpClient Client { get; set; }
        internal NetworkStream Stream { get; set; }
        internal string Address { get; set; }
    }
}
