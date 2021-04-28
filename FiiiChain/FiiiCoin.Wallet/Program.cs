using log4net;
using log4net.Config;
using System;
using System.Data.HashFunction;
using System.Data.HashFunction.Blake2;
using System.Linq;
using System.Text;

[assembly: XmlConfigurator(Watch = true)]

namespace FiiiCoin.Wallet
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static readonly IBlake2B blake2B = Blake2BFactory.Instance.Create(new Blake2BConfig()
        {
            HashSizeInBits = 256
        });

        static void Main(string[] args)
        {
            log.Debug("Hello World!");

            var value = blake2B.ComputeHash("Hello World!");

            Console.WriteLine(string.Format("Original Value: {0}", "Hello World!"));
            Console.WriteLine(string.Format("Key Value: {0}", Encoding.ASCII.GetString(blake2B.Config.Key.ToArray())));
            Console.WriteLine(string.Format("Salt Value: {0}", Encoding.ASCII.GetString(blake2B.Config.Salt.ToArray())));

            Console.WriteLine();

            Console.WriteLine(string.Format("HEX Value: {0}", value.AsHexString()));
            Console.WriteLine(string.Format("HEX Length: {0}", value.AsHexString().Length));

            Console.WriteLine();

            Console.WriteLine(string.Format("Base64 Value: {0}", value.AsBase64String()));
            Console.WriteLine(string.Format("Base64 Length: {0}", value.AsBase64String().Length));


            Console.ReadKey();
        }
    }
}
