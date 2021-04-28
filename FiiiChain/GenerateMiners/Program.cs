using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.Consensus;
using FiiiChain.Framework;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GenerateMiners
{
    class Program
    {
        /// <summary>
        /// 循环调用Node接口生成地址，然后把地址，SN，Account写进矿池数据库中
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            
            bool testnet = false;
            if (args != null && args.Length > 0)
            {
                if(args[0] == "-testnet")
                {
                    LogHelper.Info("this is testnet");
                    testnet = true;
                }
            }
            MysqlHelper helper = new MysqlHelper();
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri(testnet ? "http://127.0.0.1:5006/" : "http://127.0.0.1:5007/"), authHeaderValue, null, null, "application/json");
            RpcRequest settingRequest = RpcRequest.WithNoParameters("GetTxSettings", 1);
            RpcResponse settingResponse = await client.SendRequestAsync(settingRequest);
            if (settingResponse.HasError)
            {
                LogHelper.Error(settingResponse.Error.Message.ToString());
                throw new Exception(settingResponse.Error.Message.ToString());
            }
            GetTxSettings responseValue = settingResponse.GetResult<GetTxSettings>();
            if (responseValue.Encrypt)
            {
                string password = string.Empty;
                //加密情况下，主链一个参数，测试链二个参数
                if (testnet)
                {
                    password = args[1];
                }
                else
                {
                    password = args[0];
                }

                RpcRequest passphraseRequest = RpcRequest.WithParameterList("WalletPassphrase", new[] { password }, 1);
                RpcResponse passphraseResponse = await client.SendRequestAsync(passphraseRequest);
                if (passphraseResponse.HasError)
                {
                    LogHelper.Error(passphraseResponse.Error.Message.ToString());
                    throw new Exception(passphraseResponse.Error.Message.ToString());
                }
                if (!passphraseResponse.GetResult<bool>())
                {
                    LogHelper.Error(passphraseResponse.Error.Message.ToString());
                    throw new Exception(passphraseResponse.Error.Message.ToString());
                }
            }
            for (int i = 0; i < 30; i++)
            {
                RpcRequest request = RpcRequest.WithParameterList("GetNewAddress", new[] { "" }, 1);
                RpcResponse response = await client.SendRequestAsync(request);
                if (response.HasError)
                {
                    LogHelper.Error(response.Error.Message.ToString());
                    throw new Exception(response.Error.Message.ToString());
                }
                LogHelper.Info($"{response.Result}");
                AccountInfo account = response.GetResult<AccountInfo>();
                LogHelper.Info($"account:{Newtonsoft.Json.JsonConvert.SerializeObject(account)}");
                string accountName = $"PT{(i + 1).ToString("000")}";
                string sn = $"N3000TEST{(i + 1).ToString("000")}";
                helper.InsertMiners(account.Address, sn, accountName);
                System.Threading.Thread.Sleep(2000);
            }
            
        }
    }
}
