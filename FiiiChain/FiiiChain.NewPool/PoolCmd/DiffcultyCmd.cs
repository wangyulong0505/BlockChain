using FiiiChain.Stratum;
using FiiiChain.Stratum.PoolSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool.PoolCmd
{
    public class DiffcultyCmd : BaseCmd
    {
        public static void Send(ClientUser client, int difficulty)
        {
            SendModel model = new SendModel
            {
                Id = (int)MethodId.Diffculty,
                Method = MethodName.Diffculty_Name,
                Params = new object[] { difficulty}
            };
            PoolJob.m_socket.SendMessage(client, model);
        }

        public static void Receive(ClientUser client, object[] resultList)
        {

        }

    }
}
