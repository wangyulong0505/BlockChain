using FiiiChain.KafkaMQ;
using System;

namespace FiiiChain.KafkaMQConsumer.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MqManager.Current.Regist("test001", Get);
            MqManager.Current.Listen();
            Console.ReadLine();
        }

        public static void Get(object param)
        {
            Console.WriteLine(param.ToString());
        }
    }
}
