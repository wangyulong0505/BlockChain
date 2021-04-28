using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using System.Net;


namespace FiiiChain.NewPool.PoolCmd
{
    public class NotifyCmd : BaseCmd
    {
        public static void Send(ClientUser client, StartMsg msg, bool status)
        {
            if (msg == null) return;

            if (client.Authorized && client.Subscribed)
            {//已授权、已订阅才分配任务
                SendModel model = new SendModel
                {
                    Id = (int)MethodId.Notify,
                    Method = MethodName.Notify_Name,
                    Params = new object[] { "", msg.BlockHeight, msg.ScoopNumber, msg.StartTime, msg.GenHash, status }
                };
                PoolJob.m_socket.SendMessage(client, model);
            }
            else
            {
                LogHelper.Error($"NotifyCmd-Send,client={client.IPAddress},Authorized={client.Authorized},Subscribed={client.Subscribed}");
            }
            
        }

        public static void Receive(ClientUser client, object[] resultList)
        {

        }

    }
}
