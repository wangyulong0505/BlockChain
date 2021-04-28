// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Pool.Sockets;
using FiiiChain.PoolMessages;

namespace FiiiChain.Pool.Commands
{
    internal class StartCommand
    {
        internal static void Send(TcpState e, StartMsg startMsg)
        {
            var startCmd = PoolCommand.CreateCommand(CommandNames.Start, startMsg);
            if (PoolJob.TcpServer != null)
            {
                PoolJob.TcpServer.SendCommand(e, startCmd);
            }
        }

        internal static void Send(TcpState e)
        {
            var startMsg = new StartMsg();
            PoolTask poolCache = GetFreeMinerTask();

            var startCmd = PoolCommand.CreateCommand(CommandNames.Start, poolCache.CurrentStartMsg);
        }

        private static PoolTask GetFreeMinerTask()
        {
            PoolTask poolCache = new PoolTask();
            return poolCache;
        }
    }
}