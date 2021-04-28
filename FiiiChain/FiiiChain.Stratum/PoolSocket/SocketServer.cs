using FiiiChain.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FiiiChain.Stratum.PoolSocket
{
    public class SocketServer
    {
        private int m_maxConnectNum;    //最大连接数  
        private int m_revBufferSize;    //最大接收字节数  
        BufferManager m_bufferManager;
        const int opsToAlloc = 2;
        Socket listenSocket;            //监听Socket  
        SocketAsyncEventArgsPool m_pool;
        //int m_clientCount;              //连接的客户端数量  
        Semaphore m_maxNumberAcceptedClients;
        SocketDaemonThread m_daemonThread;

        public Action<ClientUser, int> ClientNumberChangeAction { get; set; }
        public Action<ClientUser, byte[]> ReceiveClientDataAction { get; set; }

        public List<ClientUser> m_clients { get; private set; }
        public int SocketTimeOut { get; set;}
        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="numConnections">最大连接数</param>  
        /// <param name="receiveBufferSize">缓存区大小</param>  
        public SocketServer(int numConnections, int receiveBufferSize)
        {
            //m_clientCount = 0;
            m_maxConnectNum = numConnections;
            m_revBufferSize = receiveBufferSize;   
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToAlloc, receiveBufferSize);
            m_pool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
            m_clients = new List<ClientUser>();
        }

        /// <summary>  
        /// 初始化  
        /// </summary>  
        public void Init()
        {
            m_bufferManager.InitBuffer();
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_maxConnectNum; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new ClientUser();
                m_bufferManager.SetBuffer(readWriteEventArg);
                //readWriteEventArg.SetBuffer(m_buffer, 0, m_bufferSize);
                m_pool.Push(readWriteEventArg);
            }
        }


        /// <summary>  
        /// 启动服务  
        /// </summary>  
        /// <param name="localEndPoint"></param>  
        public bool Start(IPEndPoint localEndPoint)
        {
            try
            {
                m_clients.Clear();
                listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint); 
                listenSocket.Listen(m_maxConnectNum);  
                StartAccept(null);
                //m_daemonThread = new SocketDaemonThread(this);

                if (ClientNumberChangeAction != null)
                {
                    ClientNumberChangeAction(null,0);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 停止服务  
        /// </summary>  
        public void Stop()
        {
            foreach (ClientUser client in m_clients)
            {
                try
                {
                    client.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"client Stop1 {client.IPAddress} error: {ex.Message} !");
                }
            }
            try
            {
                listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client Stop2 error: {ex.Message} !");
            }

            listenSocket.Close();
            int c_count = m_clients.Count;
            lock (m_clients) { m_clients.Clear(); }

            if (ClientNumberChangeAction != null)
                ClientNumberChangeAction(null,0);
        }


        public void CloseClient(ClientUser client)
        {
            try
            {
                client.Socket.Shutdown(SocketShutdown.Both);
                Thread.Sleep(10);
                client.Socket.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client CloseClient {client.IPAddress} error: {ex.Message} !");
            }
        }
  
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {  
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            if (!listenSocket.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }
 
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs readEventArgs = m_pool.Pop();
            ClientUser client = (ClientUser)readEventArgs.UserToken;
            client.Socket = e.AcceptSocket;
            client.ConnectTime = DateTime.Now;
            client.ActiveDateTime = DateTime.Now;
            client.Remote = e.AcceptSocket.RemoteEndPoint;
            client.IPAddress = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Address.ToString();
            try
            {
                //Interlocked.Increment(ref m_clientCount);
                lock (m_clients) { m_clients.Add(client); }
                LogHelper.Info($"client {client.Remote} connected, connetnum:{m_clients.Count}");

                if (ClientNumberChangeAction != null)
                {
                    ClientNumberChangeAction(client, 1);
                }

                if (!client.Socket.ReceiveAsync(readEventArgs))
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client ProcessAccept {client.Remote} error, message: {ex.Message}");
            }

            // Accept the next connection request  
            if (e.SocketError == SocketError.OperationAborted) return;

            StartAccept(e);
        }


        void IO_Completed(object sender, SocketAsyncEventArgs e)
        { 
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            { 
                ClientUser client = (ClientUser)e.UserToken;

                client.ActiveDateTime = DateTime.Now;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {

                    //读取数据  
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);

                    //var commandDataList = FilterToData(data);
                    //if (ReceiveClientDataAction_New != null)
                    //{
                    //    ReceiveClientDataAction_New(token, commandDataList, m_clients.Count);
                    //}

                    lock (client.Buffer)
                    {
                        client.Buffer.AddRange(data);
                    }
                    do
                    {
                        //判断包的长度,前面4个字节.
                        byte[] lenBytes = client.Buffer.GetRange(0, 4).ToArray();
                        int packageLen = BitConverter.ToInt32(lenBytes, 0);
                        if (packageLen <= client.Buffer.Count - 4)
                        {
                            byte[] rev = client.Buffer.GetRange(4, packageLen).ToArray();
                            lock (client.Buffer)
                            {
                                client.Buffer.RemoveRange(0, packageLen + 4);
                            }
 
                            if (ReceiveClientDataAction != null)
                            {
                                ReceiveClientDataAction(client, rev);
                            }

                        }
                        else
                        {   //长度不够,还得继续接收,需要跳出循环
                            break;
                        }
                    } while (client.Buffer.Count > 4);

                    //继续接收 
                    if (!client.Socket.ReceiveAsync(e))
                        this.ProcessReceive(e);
                }
                else
                {
                   CloseClientSocket(e);

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client ProcessReceive error: {ex.Message} !");
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                 
                ClientUser client = (ClientUser)e.UserToken;
                client.ActiveDateTime = DateTime.Now;
                bool willRaiseEvent = client.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        //关闭客户端  
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            try
            {
                ClientUser client = e.UserToken as ClientUser;
                lock (m_clients) { m_clients.Remove(client); }

                if (ClientNumberChangeAction != null)
                {
                    ClientNumberChangeAction(client, -1);
                }

                client.Socket.Shutdown(SocketShutdown.Send);
                client.Socket.Close();
                //Interlocked.Decrement(ref m_clientCount);
                m_maxNumberAcceptedClients.Release();

                e.UserToken = new ClientUser();
                m_pool.Push(e);

                LogHelper.Info(String.Format("client {0} disconnected!", client.Remote));
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client CloseClientSocket error: {ex.Message} !");
            }
        }

        /// <summary>  
        /// 对数据进行打包,然后再发送  
        /// </summary>  
        /// <param name="client"></param>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        private void Send(ClientUser client, byte[] message)
        {
            if (client == null || client.Socket == null || !client.Socket.Connected)
                return;
            try
            {
                //新建异步发送对象, 发送消息  
                SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                sendArg.UserToken = client;
                sendArg.SetBuffer(message, 0, message.Length);  //将数据放置进去.  
                client.Socket.SendAsync(sendArg);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"client Send error: {ex.Message} !");
            }
        }

        public void SendMessage(ClientUser client, object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] sendBuffer = StratumHelper.PackToByte(json);
            Send(client, sendBuffer);
        }
    }
}
