using System;

namespace GenerateMiningPool
{
    public class ECDSetting
    {
        public static bool IsTestNet = false;

        private const string PrivateKey_testnet = "302E020100300506032B657004220420BC4EF7256C2FE46373866E0AA28F615EBC3A4BF882B240B3BA7D07A77843F4E9";
        private const string PublicKey_testnet = "302A300506032B65700321005B6F49EEFCD80667BE21AD2437E1EDCE7A464FC6D7FDD76CBAFD40C721463577";

        private const string PrivateKey_mainnet = "302E020100300506032B657004220420436A3CAD3625858464DA44D3A42E796F7E86B973849B140C21C673BC734B0BAC";
        private const string PublicKey_mainnet = "302A300506032B657003210012D954294EC2FA1659AC7E245C2A615EFD7DFE0F3B7DF2567A968C91D557D5C7";

        public static string PublicKey
        {
            get
            {
                return IsTestNet?PublicKey_testnet:PublicKey_mainnet;
            }
        }

        public static string PrivateKey
        {
            get
            {
                return IsTestNet?PrivateKey_testnet:PrivateKey_mainnet;
            }
        }
    }
}