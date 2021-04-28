// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Pool.Sockets;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Pool.Commands
{
    internal static class StopCommand
    {
        internal static void Send(TcpState e, StopMsg stopMsg)
        {
            var stopCmd = PoolCommand.CreateCommand(CommandNames.Stop, stopMsg);
            if (PoolJob.TcpServer != null)
            {
                PoolJob.TcpServer.SendCommand(e, stopCmd);
            }
        }
    }
}
