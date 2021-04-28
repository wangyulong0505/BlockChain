using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO.Transaction;
using FiiiChain.Framework;
using System.Threading.Tasks;
using System.Threading;
using FiiiChain.Entities;
using FiiiChain.MiningPool.Business;
using FiiiChain.InvestorAward.AddressValidate;
using System.Globalization;

namespace FiiiChain.InvestorAward
{
    class Program
    {
        /// <summary>
        /// 主链和测试链区分，看后面是否有-testnet参数
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            bool testnet = false;
            if (args.Length == 1 && args[0].ToLower() == "-testnet")
            {
                testnet = true;
                LogHelper.Info("this is Testnet");
            }
            /* 设计思路
             * 1、根据总账户和投资人的信息确定要发放13笔交易的金额
             * 2、根据交易的金额，先生成13笔utxo，返回txid和vout
             * 3、等待utxo全部确认后执行发放，每一次使用一个utxo
             */
            try
            {
                //从Excel文件中获取投资人的信息
                ExcelOperation operation = new ExcelOperation();
                List<Investor> list = operation.ReadExcelFile();
                long totalAmount = 0;
                foreach (var item in list)
                {
                    //判断地址是否合法
                    if (!AddressTools.AddressVerfy(testnet ? "testnet" : "mainnet", item.Address))
                    {
                        LogHelper.Error($"{item.Name} Address is invalid, the invalid address is {item.Address} ");
                        Console.WriteLine($"{item.Name} Address is invalid, the invalid address is {item.Address} ");
                        return;
                    }
                    totalAmount += item.Amount;
                }

                //从配置文件获取解锁时间
                //List<long> lockTimeList = new List<long>() { LOCKTIME1, LOCKTIME2, LOCKTIME3, LOCKTIME4, LOCKTIME5, LOCKTIME6, LOCKTIME7, LOCKTIME8, LOCKTIME9, LOCKTIME10, LOCKTIME11, LOCKTIME12, LOCKTIME13 };
                ConfigurationTool tool = new ConfigurationTool();
                AppSetting setting = tool.GetAppSettings<AppSetting>("AppSetting");
                List<long> lockTimeList = new List<long>();
                foreach (var item in setting.TimeSetting)
                {
                    DateTime temp = Convert.ToDateTime(item.Time).ToUniversalTime();
                    lockTimeList.Add(Time.GetEpochTime(temp.Year, temp.Month, temp.Day, temp.Hour, temp.Minute, temp.Second));
                }

                //获取发送者信息，获取可用的utxo,调用GetSinceBlock接口
                //先调用GetBlockHash接口，获取0号区块之后的所有区块
                AuthenticationHeaderValue authHeaderValue = null;

                RpcClient client = new RpcClient(new Uri(testnet ? "http://localhost:5006/" : "http://localhost:5007/"), authHeaderValue, null, null, "application/json");
                RpcRequest blockRequest = RpcRequest.WithParameterList("GetBlockHash", new List<object> { 0 }, 1);
                RpcResponse blockResponse = await client.SendRequestAsync(blockRequest);
                if (blockResponse.HasError)
                {
                    LogHelper.Error(blockResponse.Error.Message);
                    throw new Exception(blockResponse.Error.Message);
                }
                string blockHash = blockResponse.GetResult<string>();
                //调用ListSinceBlock接口
                RpcRequest transactionRequest = RpcRequest.WithParameterList("ListSinceBlock", new List<object> { blockHash, 20000 }, 1);
                RpcResponse transactionResponse = await client.SendRequestAsync(transactionRequest);
                if (transactionResponse.HasError)
                {
                    LogHelper.Error(transactionResponse.Error.Message);
                    throw new Exception(transactionResponse.Error.Message);
                }
                ListSinceBlock block = transactionResponse.GetResult<ListSinceBlock>();

                List<SendRawTransactionInputsIM> sendersList = new List<SendRawTransactionInputsIM>();
                SinceBlock[] sinceBlock = block.Transactions;
                int point = 0;
                foreach (var item in sinceBlock)
                {
                    //判断amount是否满足所有人总金额的0.08
                    
                    //暂时认为手续费是1个Fiii
                    if ((item.amount + 1) > totalAmount * 0.08 && item.IsSpent == false && item.Category.ToLower() != "immature" && item.LockTime <= Time.EpochTime)
                    {
                        if (point > 12)
                        {
                            break;
                        }
                        else
                        {
                            sendersList.Add(new SendRawTransactionInputsIM() { TxId = item.TxId, Vout = Convert.ToInt32(item.Vout) });
                            point++;
                        }
                    }
                }
                if (sendersList.Count < 13)
                {
                    throw new Exception("utxo is not enough");
                }

                //先判断钱包是否加密，如果加密先解密
                RpcRequest settingRequest = RpcRequest.WithNoParameters("GetTxSettings", 1);
                RpcResponse settingResponse = await client.SendRequestAsync(settingRequest);
                if (settingResponse.HasError)
                {
                    LogHelper.Error(settingResponse.Error.Message);
                    throw new Exception(settingResponse.Error.Message);
                }
                GetTxSettings responseValue = settingResponse.GetResult<GetTxSettings>();
                if (responseValue.Encrypt)
                {
                    Console.Write("Please enter your wallet password:");
                    string password = "";
                    ConsoleKeyInfo info = Console.ReadKey(true);
                    while (info.Key != ConsoleKey.Enter)
                    {
                        Console.Write("*");
                        password += info.KeyChar;
                        info = Console.ReadKey(true);
                    }
                    Console.WriteLine();
                    if (string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine("password can not empty");
                        return;
                    }
                    //先解锁
                    RpcRequest passphraseRequest = RpcRequest.WithParameterList("WalletPassphrase", new[] { password }, 1);
                    RpcResponse passphraseResponse = await client.SendRequestAsync(passphraseRequest);
                    if (passphraseResponse.HasError)
                    {
                        LogHelper.Error(passphraseResponse.Error.Message);
                        throw new Exception(passphraseResponse.Error.Message);
                    }
                    if (!passphraseResponse.GetResult<bool>())
                    {
                        LogHelper.Error(passphraseResponse.Error.Message);
                        Console.WriteLine("wallet passphrase failure");
                        return;
                    }
                }
                //循环遍历
                for (int i = 0; i < 13; i++)
                {
                    //第一个参数发送者
                    SendRawTransactionInputsIM[] sender = new SendRawTransactionInputsIM[] { sendersList[i] };
                    //第二个参数接收者
                    SendRawTransactionOutputsIM[] receivers = new SendRawTransactionOutputsIM[list.Count];
                    LogHelper.Info($"************************begin write log for { i + 1 } transaction************************");
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (i != 12)
                        {
                            receivers[j] = new SendRawTransactionOutputsIM();
                            receivers[j].Address = list[j].Address;
                            receivers[j].Amount = Convert.ToInt64(Math.Ceiling(list[j].Amount * 0.08));
                            LogHelper.Info($"Name: {list[j].Name}, Address: {list[j].Address}, TotalAmount: {list[j].Amount}, Amount: {receivers[j].Amount}, Phone: {list[j].Phone}");
                        }
                        else
                        {
                            receivers[j] = new SendRawTransactionOutputsIM();
                            receivers[j].Address = list[j].Address;
                            receivers[j].Amount = Convert.ToInt64(Math.Ceiling(list[j].Amount * 0.04));
                            LogHelper.Info($"Name: {list[j].Name}, Address: {list[j].Address}, TotalAmount: {list[j].Amount}, Amount: {receivers[j].Amount}, Phone: {list[j].Phone}");
                        }
                    }
                    //第三个参数找零地址
                    string changeAddress = setting.ChangeAddress;
                    //第四个参数锁定时间
                    long lockTime = lockTimeList[i];
                    //第五个参数费率
                    long feeRate = setting.FeeRate;
                    LogHelper.Info($"this time the lock time is: {lockTime}");

                    //调用接口SendRawTransaction，发送交易
                    RpcRequest request = RpcRequest.WithParameterList("SendRawTransaction", new List<object> { sender, receivers, changeAddress, lockTime, feeRate }, 1);
                    RpcResponse response = await client.SendRequestAsync(request);
                    if (response.HasError)
                    {
                        LogHelper.Error(response.Error.Message);
                        throw new Exception(response.Error.Message);
                    }
                    string txHash = response.GetResult<string>();
                    //记录一下发放日志：地址，金额，期数，锁定时间，交易Hash
                    LogHelper.Info($"this transaction hash is: {txHash}");
                    LogHelper.Info("************************end write log for the transaction************************");
                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }
    }
}
