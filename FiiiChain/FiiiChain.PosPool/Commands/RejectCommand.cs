// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.PosPool.Sockets;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.PosPool.Commands
{
    internal static class RejectCommand
    {
        internal static void Send(TcpState e)
        {
            var rejectCmd = PoolCommand.CreateCommand(CommandNames.Reject, null);
            if (PoolJob.TcpServer != null)
            {
                PoolJob.TcpServer.SendCommand(e, rejectCmd);
                PoolJob.TcpServer.CloseSocket(e);
            }
        }
    }
}
