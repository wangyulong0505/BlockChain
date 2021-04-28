using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.Data;
using FiiiChain.DTO.Utxo;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.Api.Test
{
    public class ListPageUnspentTest
    {
        public async Task UTXOTest()
        {
            int index = 0;
            OutputDac dac = OutputDac.Default;
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://127.0.0.1:5007/"), authHeaderValue, null, null, "application/json");
            RpcRequest transactionRequest = RpcRequest.WithParameterList("ListPageUnspentNew", new List<object> { 1, 500 }, 1);
            RpcResponse transactionResponse = await client.SendRequestAsync(transactionRequest);
            if (transactionResponse.HasError)
            {
                LogHelper.Error(transactionResponse.Error.Message.ToString());
                throw new Exception(transactionResponse.Error.Message.ToString());
            }
            //不够的话继续获取
            ListPageUnspentOM unspent = transactionResponse.GetResult<ListPageUnspentOM>();
            //分页发放奖励
            for (int i = 0; i < 100000000; i++)
            {
                //先遍历获取可用的utxo
                while (index < unspent.UnspentOMList.Count)
                {
                    if (unspent.UnspentOMList[index].spendable == false)
                    {
                        LogHelper.Info($"avaliable txhash is {unspent.UnspentOMList[index].txid}, avaliable vout is {unspent.UnspentOMList[index].vout}");
                        //System.Threading.Thread.Sleep(3000);
                        //更新数据库状态为Spent
                        dac.UpdateSpentStatus(unspent.UnspentOMList[index].txid, unspent.UnspentOMList[index].vout);
                        index++;
                    }
                    else
                    {
                        LogHelper.Info($"unavaliable txhash is {unspent.UnspentOMList[index].txid}, avaliable vout is {unspent.UnspentOMList[index].vout}");
                        index++;
                    }

                    if (index >= unspent.UnspentOMList.Count)
                    {
                        RpcRequest tempTransactionRequest = RpcRequest.WithParameterList("ListPageUnspentNew", new List<object> { 1, 500 }, 1);
                        RpcResponse tempTransactionResponse = await client.SendRequestAsync(tempTransactionRequest);
                        if (tempTransactionResponse.HasError)
                        {
                            LogHelper.Error(tempTransactionResponse.Error.Message.ToString());
                            throw new Exception(tempTransactionResponse.Error.Message.ToString());
                        }
                        unspent = tempTransactionResponse.GetResult<ListPageUnspentOM>();
                        if (unspent.UnspentOMList.Count == 0)
                        {
                            LogHelper.Info("no avaliable txid found, start from first page again");
                            return;
                        }
                        index = 0;
                    }
                }
            }
        }
    }
}
