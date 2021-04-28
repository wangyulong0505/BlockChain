using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FiiiChain.Stratum.PoolSocket
{
    public class SocketDaemonThread
    {
        private Thread m_thread;
        private SocketServer m_socketServer;

        public SocketDaemonThread(SocketServer socketServer)
        {
            m_socketServer = socketServer;
            m_thread = new Thread(DaemonThreadStart);
            m_thread.Start();
        }

        public void DaemonThreadStart()
        {
            while (m_thread.IsAlive)
            {
                for (int i = 0; i < m_socketServer.m_clients.Count; i++)
                {
                    if (!m_thread.IsAlive)
                        break;
                    try
                    {
                        TimeSpan ts = DateTime.Now.Subtract(m_socketServer.m_clients[i].ActiveDateTime);

                        int runSeconds  =Convert.ToInt32(DateTime.Now.Subtract(m_socketServer.m_clients[i].ActiveDateTime).TotalMilliseconds);
                        if (runSeconds > m_socketServer.SocketTimeOut) //超时Socket断开
                        {
                            lock (m_socketServer.m_clients[i])
                            {
                                m_socketServer.CloseClient(m_socketServer.m_clients[i]);
                                LogHelper.Info($"Daemon thread close client:{m_socketServer.m_clients[i].Socket.RemoteEndPoint}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"Daemon thread check timeout socket error, message: {ex.Message}");
                    }
                }

                for (int i = 0; i < 60 * 1000 / 10; i++) //每分钟检测一次
                {
                    if (!m_thread.IsAlive)
                        break;
                    Thread.Sleep(10);
                }
            }
        }

        public void Close()
        {
            m_thread.Abort();
            m_thread.Join();
        }


    }
}
