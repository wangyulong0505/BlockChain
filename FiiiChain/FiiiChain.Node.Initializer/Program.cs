using System;
using System.Threading;
using log4net.Util.TypeConverters;

namespace FiiiChain.Node.Initializer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始初始化!");
            Thread thread=new Thread(Initializer.Default.Start);
            thread.Start();
            Console.ReadKey();
        }
    }
}
