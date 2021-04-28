using FiiiChain.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Api.Test
{
    /// <summary>
    /// RabbitMq集成测试
    /// </summary>
    public class RabbitMQ
    {

        public void SendMessage()
        {
            List<string> list = new List<string> { "this is just test one", "this is just test two", "this is just test three", "this is just test four", "this is just test five" };
            RabbitMqClient.ProduceMessage(list);
        }

    }
}
