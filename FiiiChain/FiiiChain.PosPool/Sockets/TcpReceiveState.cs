// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

namespace FiiiChain.PosPool.Sockets
{
    internal class TcpReceiveState : TcpState
    {
        internal byte[] Buffer { get; set; }
    }
}
