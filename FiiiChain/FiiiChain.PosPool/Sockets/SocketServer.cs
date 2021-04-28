﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.PosPool.Commands;
using FiiiChain.PosPool.Models;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiiiChain.PosPool.Sockets
{

    class ReceiveCommandModel
    {
        public TcpReceiveState TcpReceviceStateData { get; set; }

        public PoolCommand PoolCommandData { get; set; }
    }

    internal class SocketServer
    {
        TcpListener listener = null;
        int m_receiveBufferSize;

        bool isStarted = false;
        Queue<TcpSendState> sendCommandQueue;

        // Thread signal.
        static ManualResetEvent tcpClientSend = new ManualResetEvent(false);
        static ManualResetEvent tcpClientConnect = new ManualResetEvent(false);

        public Action<TcpReceiveState, PoolCommand> ReceivedCommandAction { get; set; }
        public Func<TcpState, bool, bool> ReceivedMinerConnectionAction { get; set; }
        public int ConnectCount;

        public SocketServer(int receiveBufferSize)
        {
            m_receiveBufferSize = receiveBufferSize;
            sendCommandQueue = new Queue<TcpSendState>();
        }

        public void Start(IPEndPoint localEndPoint)
        {
            this.listener = new TcpListener(localEndPoint);
            this.listener.Start(/*Setting.Max_TCP_Count*/);
            this.isStarted = true;

            this.startAccept();
        }

        public void Stop()
        {
            this.isStarted = false;
            this.listener.Stop();
        }

        public void CloseSocket(TcpState e)
        {
            Interlocked.Decrement(ref ConnectCount);
            ReceivedMinerConnectionAction?.Invoke(e, false);

            if (e.Stream != null)
            {
                try
                {
                    e.Stream.Close();
                }
                catch
                {

                }
            }

            try
            {
                e.Client.Client.Shutdown(SocketShutdown.Both);
                e.Client.Client.Close();
                e.Client.Client = null;
                e.Client.Close();
            }
            catch
            {

            }
        }

        private void startAccept()
        {
            this.listener.BeginAcceptTcpClient(new AsyncCallback(processConnect2), this.listener);
        }

        public void SendCommand(TcpState state, PoolCommand command)
        {
            tcpClientSend.Set();
            try
            {
                var buffer = command.GetBytes();
                state.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(processSend), state);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                this.CloseSocket(state);
            }

            tcpClientSend.WaitOne();
        }

        private void processConnect(IAsyncResult ar)
        {
            try
            {
                tcpClientConnect.Set();
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient client = listener.EndAcceptTcpClient(ar);
                client.ReceiveBufferSize = m_receiveBufferSize;
                Console.WriteLine(client.Client.RemoteEndPoint);
                var stream = client.GetStream();
                TcpReceiveState state = new TcpReceiveState();
                state.Client = client;
                state.Address = client.Client.RemoteEndPoint.ToString();
                state.Stream = stream;
                state.Buffer = new byte[m_receiveBufferSize];

                Interlocked.Increment(ref ConnectCount);
                if (this.ReceivedMinerConnectionAction != null)
                {
                    if (ReceivedMinerConnectionAction(state, true))
                    {
                        state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(processReceive), state);
                    }
                }

                tcpClientConnect.WaitOne();
            }
            catch (Exception ex)
            {
                LogHelper.Error("error on processConnect", ex);
            }
            finally
            {
                this.startAccept();
            }
        }


        private void processConnect2(IAsyncResult ar)
        {
            ThreadPool.QueueUserWorkItem<IAsyncResult>(Execute, ar, false);
        }

        private void Execute(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient client = listener.EndAcceptTcpClient(ar);
                client.ReceiveBufferSize = m_receiveBufferSize;
                //Console.WriteLine("Execute " + client.Client.RemoteEndPoint);
                var stream = client.GetStream();
                TcpReceiveState state = new TcpReceiveState();
                state.Client = client;
                state.Address = client.Client.RemoteEndPoint.ToString();
                state.Stream = stream;
                state.Buffer = new byte[m_receiveBufferSize];

                Interlocked.Increment(ref ConnectCount);
                if (this.ReceivedMinerConnectionAction != null)
                {
                    if (ReceivedMinerConnectionAction(state, true))
                    {
                        state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(processReceive), state);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("error on processConnect", ex);
            }
            finally
            {
                this.startAccept();
            }
        }

        public readonly static Queue<ReceiveCommandModel> _queue = new Queue<ReceiveCommandModel>();

        private void processReceive(IAsyncResult ar)
        {
            TcpReceiveState state = (TcpReceiveState)ar.AsyncState;
            try
            {
                int numberOfBytesRead = state.Stream.EndRead(ar);

                if (numberOfBytesRead > 0)
                {
                    try
                    {
                        if (numberOfBytesRead > 28)
                            LogHelper.Debug($"Readed {numberOfBytesRead} byte data from {state.Address}");
                        var buffer = new byte[numberOfBytesRead];
                        Array.Copy(state.Buffer, 0, buffer, 0, buffer.Length);

                        var commandDataList = new List<byte[]>();
                        var index = 0;
                        List<byte> bytes = null;

                        while (index < buffer.Length)
                        {
                            if (bytes == null)
                            {
                                if ((index + 3) < buffer.Length &&
                                buffer[index] == PoolCommand.DefaultPrefixBytes[0] &&
                                buffer[index + 1] == PoolCommand.DefaultPrefixBytes[1] &&
                                buffer[index + 2] == PoolCommand.DefaultPrefixBytes[2] &&
                                buffer[index + 3] == PoolCommand.DefaultPrefixBytes[3])
                                {
                                    bytes = new List<byte>();
                                    bytes.AddRange(PoolCommand.DefaultPrefixBytes);
                                    index += 4;
                                }
                                else
                                {
                                    index++;
                                }
                            }
                            else
                            {
                                if ((index + 3) < buffer.Length &&
                                buffer[index] == PoolCommand.DefaultSuffixBytes[0] &&
                                buffer[index + 1] == PoolCommand.DefaultSuffixBytes[1] &&
                                buffer[index + 2] == PoolCommand.DefaultSuffixBytes[2] &&
                                buffer[index + 3] == PoolCommand.DefaultSuffixBytes[3])
                                {
                                    bytes.AddRange(PoolCommand.DefaultSuffixBytes);
                                    commandDataList.Add(bytes.ToArray());
                                    bytes = null;

                                    index += 4;
                                }
                                else
                                {
                                    bytes.Add(buffer[index]);
                                    index++;
                                }
                            }
                        }

                        foreach (var data in commandDataList)
                        {
                            try
                            {
                                var cmd = PoolCommand.ConvertBytesToMessage(data);
                                if (cmd != null)
                                {
                                    HeartbeatCommand.UpdateHeartTime(state);
                                    //_queue.Enqueue(new ReceiveCommandModel()
                                    //{
                                    //    TcpReceviceStateData = state,
                                    //    PoolCommandData = cmd
                                    //});
                                    PoolJob.TcpServer.ReceivedCommandAction(state, cmd);
                                }
                                cmd = null;
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("Error occured on deserialize messgae: " + ex.Message, ex);
                            }
                        }
                        commandDataList.Clear();
                        buffer = null;
                    }
                    catch
                    {

                    }
                }
                //Task.Delay(500).Wait();
                try
                {
                    state.Buffer = new byte[m_receiveBufferSize];
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(processReceive), state);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error occured on receive messgae: " + ex.Message, ex);
                this.CloseSocket(state);
            }
        }

        private void startSend()
        {
            while (isStarted)
            {
                if (this.sendCommandQueue.Count > 0)
                {

                    var item = this.sendCommandQueue.Dequeue();
                    if (item != null)
                    {
                        var buffer = item.Command.GetBytes();
                        item.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(processSend), item.Stream);
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void processSend(IAsyncResult ar)
        {
            var state = (TcpState)ar.AsyncState;

            try
            {
                state.Stream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error occured on send messgae: " + ex.Message);
                this.CloseSocket(state);
            }

            tcpClientSend.Set();
        }
    }
}
