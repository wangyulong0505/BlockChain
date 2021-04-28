using FiiiChain.KafkaMQ;
using System;

namespace FiiiChain.KafkaProducer.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MqManager.Current.Send("test001", "this is a test message, we can sen it to everyone,weilejaDJQA DAL DJahd aLDHHA d A HDHA d a Dadh akshdfkas fas fas");
            Console.ReadLine();
        }
    }
}
