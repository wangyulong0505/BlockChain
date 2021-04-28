// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Pool.Sockets
{
    internal class TcpSendState : TcpState
    {
        internal PoolCommand Command { get; set; }
    }
}
