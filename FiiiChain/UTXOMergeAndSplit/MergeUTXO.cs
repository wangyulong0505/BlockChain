using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO;
using FiiiChain.DTO.Transaction;
using FiiiChain.DTO.Utxo;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace UTXOMergeAndSplit
{
    /// <summary>
    /// UTXO合并操作
    /// </summary>
    public class MergeUTXO
    {
        AuthenticationHeaderValue authHeaderValue = null;
        List<SendRawTransactionInputsIMExtend> sendersExtendList = new List<SendRawTransactionInputsIMExtend>();
        List<SendRawTransactionInputsIM> sendersList = new List<SendRawTransactionInputsIM>();
        public async Task Merge(bool isTestNet, string password = null)
        {
            try
            {
                //配置文件中获取合并配置
                ConfigurationTool tool = new ConfigurationTool();
                MergeConfig config = tool.GetAppSettings<MergeConfig>("MergeConfig");

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
                LogHelper.Info("==========begin to list page unspent by amount==========");
                do
                {
                    RpcRequest request = RpcRequest.WithParameterList("ListPageUnspentByAmount", new List<object> { i, config.PageSize, config.SearchMaxAmount, config.SearchMinAmount }, 1);
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
                LogHelper.Info($"=========================utxo count is {sendersExtendList.Count}===============================");
                //组织SendRawTransaction参数
                List<SendRawTransactionOutputsIM> receiverList = new List<SendRawTransactionOutputsIM>();
                //receiverList.Add(new SendRawTransactionOutputsIM { Address = config.SendAddress, Amount = config.FinalAmount * 100000000 });
                long temp = 0;
                int j = 0;
                int k = 0;
                LogHelper.Info("==========begin to send raw transaction==========");
                foreach (var item in sendersExtendList)
                {
                    sendersList.Add(new SendRawTransactionInputsIM { TxId = item.TxId, Vout = item.Vout });
                    j++;
                    temp += item.Amount;
                    if(temp > config.FinalAmount)
                    {
                        //计算手续费和找零:input个数为i，output个数只有一个
                        double charge = ((68 + 262 * j + 101 * 1) / 1024.0) * config.FeeRate;
                        receiverList.Add(new SendRawTransactionOutputsIM { Address = config.SendAddress, Amount = (long)Math.Ceiling(temp - charge) });
                        //发送SendRawTransaction
                        RpcRequest request = RpcRequest.WithParameterList("SendRawTransaction", new List<object> { sendersList.ToArray(), receiverList.ToArray(), config.ChangeAddress, 0, config.FeeRate }, 1);
                        RpcResponse response = await client.SendRequestAsync(request);
                        if (response.HasError)
                        {
                            LogHelper.Error(response.Error.Message.ToString());
                        }
                        string txHash = response.GetResult<string>();
                        LogHelper.Info($"the transaction hash is: {txHash}");
                        k++;
                        if(config.TransactionCount > 0 && k >= config.TransactionCount)
                        {
                            LogHelper.Info($"the transaction count is: {k}, has large than the max limit, the program will stop");
                            LogHelper.Info("==========send success==========");
                            return;
                        }
                        temp = 0;
                        j = 0;
                        sendersList.Clear();
                        receiverList.Clear();
                        //发送交易休息时间
                        System.Threading.Thread.Sleep(config.SleepSeconds);
                    }
                }
                LogHelper.Info("==========send success==========");
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
    }
}
