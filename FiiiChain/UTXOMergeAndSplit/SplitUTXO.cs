using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO;
using FiiiChain.DTO.Transaction;
using FiiiChain.DTO.Utxo;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UTXOMergeAndSplit
{
    public class SplitUTXO
    {
        AuthenticationHeaderValue authHeaderValue = null;
        List<SendRawTransactionInputsIMExtend> sendersExtendList = new List<SendRawTransactionInputsIMExtend>();
        List<SendRawTransactionInputsIM> sendersList = new List<SendRawTransactionInputsIM>();
        public async Task Split(bool isTestNet, string password = null)
        {
            //先解密
            try
            {
                //配置文件中获取拆分配置
                ConfigurationTool tool = new ConfigurationTool();
                MergeConfig config = tool.GetAppSettings<MergeConfig>("SplitConfig");

                //先解密
                LogHelper.Info("==========begin to decrypt==========");
                RpcClient client = new RpcClient(new Uri(isTestNet ? "http://127.0.0.1:5006/" : "http://127.0.0.1:5007/"), authHeaderValue, null, null, "application/json");
                RpcRequest settingRequest = RpcRequest.WithNoParameters("GetTxSettings", 1);
                RpcResponse settingResponse = await client.SendRequestAsync(settingRequest);
                if (settingResponse.HasError)
                {
                    LogHelper.Error(settingResponse.Error.Message.ToString());
                    throw new Exception(settingResponse.Error.Message.ToString());
                }
                GetTxSettingsOM responseValue = settingResponse.GetResult<GetTxSettingsOM>();
                if (responseValue.Encrypt)
                {
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
                //调用ListPageUnspentByAmount
                int i = 1;
                ListPageUnspentOM unspent = null;
                LogHelper.Info("==========begin to List Page Unspent By Amount==========");
                do
                {
                    RpcRequest request = RpcRequest.WithParameterList("ListPageUnspentByAmount", new List<object> { i, config.PageSize, config.SearchMaxAmount * 100000000, config.SearchMinAmount * 100000000 }, 1);
                    RpcResponse response = await client.SendRequestAsync(request);
                    if (response.HasError)
                    {
                        LogHelper.Error(response.Error.Message.ToString());
                    }
                    unspent = response.GetResult<ListPageUnspentOM>();
                    if (unspent.UnspentOMList.Count > 0)
                    {
                        foreach (ListUnspentOM item in unspent.UnspentOMList)
                        {
                            sendersExtendList.Add(new SendRawTransactionInputsIMExtend { TxId = item.txid, Vout = item.vout, Amount = item.amount });
                        }
                    }
                    i++;
                }
                while (unspent.UnspentOMList.Count > 0);

                LogHelper.Info($"==========utxo count is {sendersExtendList.Count}==========");
                //组织SendRawTransaction参数
                long temp = 0;
                LogHelper.Info("==========begin to Send Raw Transaction==========");
                foreach (SendRawTransactionInputsIMExtend item in sendersExtendList)
                {
                    List<SendRawTransactionOutputsIM> receiverList = new List<SendRawTransactionOutputsIM>();
                    temp = item.Amount;
                    while (temp > config.FinalAmount * 100000000 + 100000000)
                    {
                        receiverList.Add(new SendRawTransactionOutputsIM { Address = config.SendAddress, Amount = config.FinalAmount * 100000000 });
                        temp -= config.FinalAmount * 100000000;
                    }
                    //发送SendRawTransaction
                    sendersList.Add(new SendRawTransactionInputsIM { TxId = item.TxId, Vout = item.Vout });
                    RpcRequest request = RpcRequest.WithParameterList("SendRawTransaction", new List<object> { sendersList.ToArray(), receiverList.ToArray(), config.ChangeAddress, 0, config.FeeRate }, 1);
                    RpcResponse response = await client.SendRequestAsync(request);
                    if (response.HasError)
                    {
                        LogHelper.Error(response.Error.Message.ToString());
                    }
                    string txHash = response.GetResult<string>();
                    LogHelper.Info($"the transaction hash is: {txHash}");
                    temp = 0;
                    sendersList.Clear();
                }
                LogHelper.Info("==========end to Send Raw Transaction==========");
                LogHelper.Info("==========Send success==========");
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
    }
}
