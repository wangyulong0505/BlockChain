// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using FiiiChain.PosPool.Commands;
using FiiiChain.PosPool.Models;
using FiiiChain.PosPool.Sockets;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace FiiiChain.PosPool
{
    public class PoolJob
    {
        internal static SocketServer TcpServer = null;

        public PoolJob()
        {
            TcpServer = new SocketServer(10 * 1024);
        }

        public void Start()
        {
            IPEndPoint iP = new IPEndPoint(IPAddress.Any, Setting.PoolPort);
            TcpServer.Start(iP);

            TcpServer.ReceivedCommandAction = receivedCommand;
            TcpServer.ReceivedMinerConnectionAction = receivedConnection;            
        }

        Timer listenTimer;
        public void StartListen()
        {
            listenTimer = new Timer(1000);
            listenTimer.Elapsed += ListenTimer_Elapsed;
            listenTimer.Start();
        }

        private void ListenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            listenTimer.Stop();
            try
            {
                if (PoolCache.CurrentTask != null && PoolCache.CurrentTask.State == MiningState.Mining)
                    return;

                if (PoolCache.CurrentTask == null && PoolCache.poolTasks.Any())
                    PoolCache.CurrentTask = PoolCache.poolTasks.FirstOrDefault();

                PoolCache.poolTasks.Remove(PoolCache.CurrentTask);

                if (PoolCache.CurrentTask != null && PoolCache.CurrentTask.State == MiningState.Wait)
                {
                    foreach (Miner miner in PoolCache.WorkingMiners)
                    {
                        try
                        {
                            NonceDataCommand.Send(new TcpState { Client = miner.Client, Stream = miner.Stream, Address = miner.ClientAddress });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.ToString());
                            //PoolCache.WorkingMiners.Remove(miner);
                        }
                    }
                    PoolCache.CurrentTask.State = MiningState.Mining;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                listenTimer.Start();
            }
        }

        private void receivedCommand(ReceiveCommandModel model)
        {
            receivedCommand(model.TcpReceviceStateData, model.PoolCommandData);
        }

        private void receivedCommand(TcpReceiveState state, PoolCommand cmd)
        {
            LogHelper.Info(cmd.CommandName);
            switch (cmd.CommandName)
            {
                case CommandNames.Login:
                    Commands.LoginCommand.Receive(state, cmd);
                    break;
                case CommandNames.NonceData:
                    Commands.NonceDataCommand.Receive(state, cmd);
                    break;
                case CommandNames.ScoopData:
                    //Commands.ScoopDataCommand.Received(state, cmd);
                    break;
                case CommandNames.Heartbeat:
                    Commands.HeartbeatCommand.Receive(state, cmd);
                    break;
                default:
                    break;
            }
        }

        private bool receivedConnection(TcpState e, bool connected)
        {
            try
            {
                var miner = PoolCache.WorkingMiners.FirstOrDefault(m => m.ClientAddress == e.Address);

                if (connected)
                {
                    if (miner != null && miner.IsConnected)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (miner != null)
                    {
                        miner.IsConnected = false;
                        //PoolCache.WorkingMiners.Remove(miner);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }

            return false;
        }
    }
}
