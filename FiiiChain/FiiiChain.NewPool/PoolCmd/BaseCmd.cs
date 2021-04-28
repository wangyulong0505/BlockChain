using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool.PoolCmd
{
    public abstract class BaseCmd
    {
        protected static void Reply(ClientUser client,MethodId id, object[] result, int errorCode = 0)
        {
            ReplyModel model = new ReplyModel()
            {
                Id = (int)id,
                Result = result,
                Error = errorCode
            };
            PoolJob.m_socket.SendMessage(client, model);
        }
    }
}
