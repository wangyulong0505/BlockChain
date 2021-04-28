using FiiiChain.Framework;
using FiiiChain.NewPool.PoolCmd;
using FiiiChain.PoolMessages;
using FiiiChain.RabbitMQ;
using FiiiChain.ShareModels;
using FiiiChain.ShareModels.Msgs;
using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FiiiChain.NewPool
{
    public class PoolJob
    {
        internal static SocketServer m_socket;

        public PoolJob()
        {
            m_socket = new SocketServer(ReadConfig.SocketConfig.MaxConnectNum, ReadConfig.SocketConfig.BufferSize);
            m_socket.SocketTimeOut = ReadConfig.SocketConfig.ClientTimeOut;
            m_socket.Init();
        }

        public void Start()
        {
            IPEndPoint iP = new IPEndPoint(IPAddress.Any,ReadConfig.SocketConfig.GetPort());
            bool result = m_socket.Start(iP);
            LogHelper.Info($"PoolJob Start Status:{result}!");
            m_socket.ReceiveClientDataAction = ReceivedCommand;
            m_socket.ClientNumberChangeAction = ReceivedClientConnecion;
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
                    foreach (ClientUser miner in m_socket.m_clients)
                    {//矿池分配任务
                        try
                        {
                            //新任务下发
                            NotifyCmd.Send(miner, PoolCache.CurrentTask.CurrentStartMsg, false);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error($"PoolJob-ListenTimer_Elapsed send error:{ex.ToString()}");
                        }
                    }
                    PoolCache.CurrentTask.State = MiningState.Mining;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"PoolJob-ListenTimer_Elapsed error:{ex.ToString()}");
            }
            finally
            {
                listenTimer.Start();
            }
        }

        private void ReceivedCommand(ClientUser client, byte[] data)
        {
            Task.Factory.StartNew(() => {

               string info = Encoding.UTF8.GetString(data);
               LogHelper.Info($"ReceivedCommand:{info}!");

               DataModel dataModel = JsonConvert.DeserializeObject<DataModel>(info);
               switch (dataModel.Id)
               {
                    case (int)MethodId.Authorize:
                        AuthorizeCmd.Receive(client, dataModel.Params); //收到授权请求
                        break;
                    case (int)MethodId.Subscribe:
                        SubscribeCmd.Receive(client, dataModel.Params); //收到订阅请求
                        break;
                    case (int)MethodId.Notify:
                        NotifyCmd.Receive(client, dataModel.Result);    //收到分配任务回复
                        break;
                    case (int)MethodId.Submit:
                        SubmitCmd.Receive(client, dataModel.Params);    //收到提交结果请求
                        break;
                    case (int)MethodId.Diffculty:
                        DiffcultyCmd.Receive(client, dataModel.Result); //收到难度调整回复
                        break;
                    default:
                        break;
               }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="status"></param>
        private void ReceivedClientConnecion(ClientUser client, int status)
        {
            Task.Factory.StartNew(() => {
                
                LogHelper.Info($"ReceivedClientConnecion:{client.IPAddress} ,Status:{(status==1?"login":"exit")},clients:{m_socket.m_clients.Count}!");

            });
        }
    }
}
