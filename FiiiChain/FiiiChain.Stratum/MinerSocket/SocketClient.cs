using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using FiiiChain.Framework;
using FiiiChain.Stratum.PoolSocket;

namespace FiiiChain.Stratum.MinerSocket
{
    /// <summary>
    /// 异步TCP客户端
    /// </summary>
    public class SocketClient : IDisposable
    {
        #region Fields

        private TcpClient tcpClient;
        private bool disposed = false;
        //private int retries = 0; //尝试连接次数

        #endregion

        #region Ctors

        /// <summary>
        /// 异步TCP客户端
        /// </summary>
        /// <param name="ep"></param>
        public SocketClient(IPEndPoint ep)
        {
            this.RemoteEP = ep;
            this.tcpClient = new TcpClient(AddressFamily.InterNetwork);
            this.Encoding = Encoding.UTF8;

            //Retries = 3;
            //RetryInterval = 5;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 是否已与服务器建立连接
        /// </summary>
        public bool Connected { get { return tcpClient.Client.Connected; } }
        /// <summary>
        /// 远端服务器
        /// </summary>
        public IPEndPoint RemoteEP { get; private set; }
        /// <summary>
        /// 通信所使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 连接重试次数
        /// </summary>
        public int Retries { get; set; }
        /// <summary>
        /// 连接重试间隔
        /// </summary>
        public int RetryInterval { get; set; }
        #endregion

        #region Connect

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <returns>异步TCP客户端</returns>
        public void Connect()
        {
            try
            {
                // start the async connect operation
                tcpClient.BeginConnect(
                  RemoteEP.Address, RemoteEP.Port, ProcessConnect, tcpClient);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"{RemoteEP.Address}:{RemoteEP.Port}Connect失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 关闭与服务器的连接
        /// </summary>
        /// <returns>异步TCP客户端</returns>
        public void Close()
        {
            if (Connected)
            {
                //retries = 0;
                tcpClient.Close();
            }
            ConnectStatusChangedAction?.Invoke(false);
        }

        #endregion

        #region Receive
        private void ProcessConnect(IAsyncResult ar)
        {
            try
            {
                tcpClient.EndConnect(ar);

                // we are connected successfully and start asyn read operation.
                byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
                tcpClient.GetStream().BeginRead(
                  buffer, 0, buffer.Length, ProcessReceive, buffer);

                if (Connected && ConnectStatusChangedAction != null)
                {
                    ConnectStatusChangedAction(true);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error($"client ProcessConnect error:{ex.Message}!");
            }
        }

        private void ProcessReceive(IAsyncResult ar)
        {
            if (!tcpClient.Connected) return;

            NetworkStream stream = tcpClient.GetStream();
            int numberOfReadBytes = 0;
            try
            {
                numberOfReadBytes = stream.EndRead(ar);
            }
            catch(Exception ex)
            {
                numberOfReadBytes = 0;
                Close();
                LogHelper.Error($"client ProcessReceive error:{ex.Message}!");
                return;
            }
            // received byte and trigger event notification
            byte[] buffer = (byte[])ar.AsyncState;
            byte[] receivedBytes = new byte[numberOfReadBytes];
            Buffer.BlockCopy(buffer, 0, receivedBytes, 0, numberOfReadBytes);
            
            //这里带更新
            //var commandDataList = FilterToData(receivedBytes);
            //if (commandDataList.Count > 0 && ReceiveServiceDataAction_New != null)
            //{
            //    ReceiveServiceDataAction_New(tcpClient, commandDataList);
            //}

            // then start reading from the network again
            stream.BeginRead(buffer, 0, buffer.Length, ProcessReceive, buffer);
        }

        #endregion

        #region Actions
        public Action<TcpClient, List<byte[]>> ReceiveServiceDataAction_New { get; set; }
        public Action<bool> ConnectStatusChangedAction;
        public Action<byte[]> ReceivedCommandAction;
        public Action<int> ProcessErrorAction;

        #endregion

        #region Send

        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="datagram">报文</param>
        public void Send(byte[] datagram)
        {
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }

            if (!Connected)
            {
                ConnectStatusChangedAction?.Invoke(false);
                return;    
            }

            try
            {
                tcpClient.GetStream().BeginWrite(datagram, 0, datagram.Length, ProcessSend, tcpClient);
            }
            catch (Exception ex)
            {
                Close();
                LogHelper.Error($"client Send error：{ex.Message}!");
            }
        }

        private void ProcessSend(IAsyncResult ar)
        {
            ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
        }

        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="datagram">报文</param>
        public void Send(string datagram)
        {
            Send(this.Encoding.GetBytes(datagram));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed 
        /// and unmanaged resources; <c>false</c> 
        /// to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Close();
                    if (tcpClient != null)
                    {
                        tcpClient = null;
                    }
                }
                disposed = true;
            }
        }

        #endregion

    }

}
