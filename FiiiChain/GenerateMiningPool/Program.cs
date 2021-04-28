using NSec.Cryptography;
using SimpleBase;
using System;
using System.Linq;

namespace GenerateMiningPool
{
    class Program
    {
        static readonly string[] NetTypes = new string[] { "-testnet", "-mainnet" };

        static void Main(string[] args)
        {
            try
            {
                if (args == null || args.Length != 2 || !NetTypes.Contains(args[0].ToLower()))
                {
                    CommandHelper();
                    return;
                }

                if (args[0].ToLower() == NetTypes[0])
                {
                    ECDSetting.IsTestNet = true;
                }
                else
                {
                    ECDSetting.IsTestNet = false;
                }
            }
            catch
            {
                CommandHelper();
                return;
            }


            string walletPublicKey = args[1];

            var generateMiningPoolKey = GenerateSignature(walletPublicKey);
            Console.WriteLine();
            Console.WriteLine(string.Format("Generate MiningPool({0}) Key =", ECDSetting.IsTestNet ? "testnet" : "mainNet"));
            Console.WriteLine(generateMiningPoolKey);
        }

        private static void CommandHelper()
        {
            Console.WriteLine("dotnet GenerateMiningPool <NetType> <WalletPublicKey>");
            Console.WriteLine("NetType is [-testnet] or [-mainnet]");
            Console.WriteLine("WalletPublicKey is Wallet PublicKey");
        }

        private static string GenerateSignature(string walletPublicKey)
        {
            try
            {
                var rkey = Base16.Decode(ECDSetting.PrivateKey).ToArray();
                var dsa2 = ECD.ImportPrivateKey(rkey);

                var bytes = Base16.Decode(walletPublicKey).ToArray();
                var encodeStr = dsa2.SingnData(bytes);

                var code = Base16.EncodeUpper(encodeStr);
                return code;
            }
            catch
            {
                Console.WriteLine("wallet PublicKey is invalid");
            }
            return null;
        }
    }
}
