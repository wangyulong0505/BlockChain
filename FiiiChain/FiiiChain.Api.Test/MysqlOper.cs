using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO.Transaction;
using FiiiChain.DTO.Utxo;
using FiiiChain.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FiiiChain.Api.Test
{
    public class MysqlOper
    {
        public const string CacheConnectionString = "server=rm-3nsnh13p454nv0r68.mysql.rds.aliyuncs.com;port=3306;user=fiiipool_dbo;password=8&KujS!sCw3Ex26f;database=fiiipool_main;SslMode=none;ConnectionTimeout=1200";
        //public const string CacheConnectionString = "server=localhost;port=3306;user=root;password=123456;database=fiiipool_main;SslMode=none;ConnectionTimeout=1200";
        public const string SqliteConnectionString = "Filename=./fiiichain.db; Mode=ReadWriteCreate;Cache=Shared;";

        public async Task GetMinersRewardByTxid(List<string> txids, List<string> tableName, List<string> paidFilePaths, List<string> unpaidFilePath, string password)
        {
            try
            {
                Dictionary<string, long> dic = new Dictionary<string, long>();
                foreach (var txid in txids)
                {
                    foreach (var item in tableName)
                    {
                        LogHelper.Info($"Get All Paid Reward For {item} by {txid}");
                        string SQL_STATEMENT =
                        $"SELECT SUM(ActualReward), MinerAddress FROM {item} WHERE Paid = 1 AND TransactionHash = '{txid}' AND MinerAddress NOT IN('fiiima17R12zutpuuGJBz7kA2AgYyupAnUhf3s','fiiima3rFfSuxcHKUyEMvTg95HjgcfXsCGLaht','fiiima7CzGa1PBqKv5v1cXBcHtB2n435dQ8QNi','fiiima8N4nGsR1mqarEejJVzfC3PZFiBwxjwiB','fiiima8Yi5r95LjjL9rNQfNxYNafPGs2gGCDof','fiiima9VDxXqKQiVvXjfeqtwCgmQDjAsFkLngn','fiiimaAP5PrkJ2jGo7PmjBXRNRh1m28o9v834o','fiiimAuKSmUrhs9LPp1FyWSxeoHfMcTvayUqHQ','fiiimB6vCSbFpNqaQiQnqQAC5kifmEGZYH4656','fiiimB8eTEQuMdAvU1wGFTiTugHy5icXuWgvy5','fiiimB9X8F5R6mHLdTR6vUcgTJMxMtmin1zKgM','fiiimBb7vamNvPbq4sMsBNFbU7VJt69oYEUArj','fiiimBb9LFWm837XCw4N7eiaQ7CrhNho3wWFeG','fiiimBcZqoRcaBL5KpaW9WvQu9MVb773cvCJFJ','fiiimBd53F3uWxipQQmHWBqJmYuanF7q3H8dCf','fiiimBizRL29hBbod5bqhxmQmL6BoH5734ib3B','fiiimBjAzdHKUMtVvaXVYQV8DCLbZUWgeWyAQg','fiiimBJkFwYEMEgJ3onSXqVHT8Wu4r9ZX6WEDo','fiiimBkuWR6XnLUVkWtK389cuhG6DGNRMx56CR','fiiimBmWgMcjJsAnd44Jbys6D3LDCyNSjD1EU9','fiiimBnxRgjZe4BJiVfHazLf8yFtyzbntWxjfp','fiiimBq4voW7dk5KjnB8DviaWLU3ijZMkQ9ZB8','fiiimBrZUVjBrHK5CPFCh3t6W6gn2ueJE7iJg9','fiiimBS3qwmb8ncpstEp8WLJsRbsE2g1hJnq8g','fiiimBsdPa1d5TEHLrJReyDMgkdM8xJyt9hPyC','fiiimBsVBNgTHMAHEcrdDVhbybHnLBaux9zz4a','fiiimBtu7TapXptHvc4hnmpxiHo4wtdEeWWuyh','fiiimBUnsu8TPMHShBsEukR1W4wHaqXZJTLeEZ','fiiimBusa9w4SVS7dMBed9mc7qfGjvL29SHQCF','fiiimBW7EsNM2UHnZVs44QwZdLaBp2WgaTzYy6','fiiimBxydvfcDWwGiHfn1QuFe5kGikaKiEDHXu','fiiimBXYMnAafJE53UYTiDX2GKsfiWuWmv2AqQ','fiiimC3mMsaaG4Vm6XpzYAWWLoCsUGLu6qWMZ6','fiiimC7dR7h553sskntyBPrbUtPMMTfCsf294i','fiiimCA7rDSZn8NJPbiraEvVcMPj3VTXyFZxEG','fiiimCaf4HmzMy5Gqf2wpP57R6HQUZRc8FB3jM','fiiimCcc3YhCqkogFgNwaa1K2MaZWUNiYvyqwV','fiiimCE2ggoaLCsxUsgNrYD3akMqxEnxTob6Er','fiiimCekz5L4wkYEtmymgGb2MMYZ9c1t3325Sx','fiiimCGwQa41QE65VSKhpSmiuDLZNKmmao68WW','fiiimCHEPNAr8YNoVPUTAVH8hKwx6m2SeyCw1y','fiiimChuWq1X5XtkiyHAPPGAGHCVArfMxnvGmK','fiiimCimpmQBRQyGZsS1XcRZzHhs9XUTZEw3t4','fiiimCJBSaBxSJaKZ9wuhXJ16kpkNQBTR7hQfR','fiiimCnS7VB5EatcTenEbk9Zi2qB6pGdpzUtY6','fiiimCpG7guZ5QvS8hfcNTkwYXtBrvh4cSDRDh','fiiimCpjF4MmuMybqjQojY54cehKyHd54krWTB','fiiimCpmtasu6brF3cGmGEzSHs8YTDAZQio4zk','fiiimCpskcXD8n7Jvq7En3y6xrmxVM5ZzTv26B','fiiimCsYtiyriBMxcX9igxrAZrZqFeuUr4iobi') GROUP BY MinerAddress;";
                        using (MySqlConnection conn = new MySqlConnection(CacheConnectionString))
                        {
                            using (MySqlCommand cmd = new MySqlCommand(SQL_STATEMENT, conn))
                            {
                                cmd.Connection.Open();
                                cmd.CommandTimeout = 1200;
                                using (MySqlDataReader dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        if (dic.ContainsKey(dr.GetString(1)))
                                        {
                                            dic[dr.GetString(1)] += dr.GetInt64(0);
                                        }
                                        else
                                        {
                                            dic.Add(dr.GetString(1), dr.GetInt64(0));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //创建一个用于遍历的key集合防止遍历中修改dictionary报错
                string[] keys = new string[dic.Keys.Count];
                dic.Keys.CopyTo(keys, 0);
                LogHelper.Info($"=================================begin to delete paid amount==============================================");
                //排除已经发过的
                Dictionary<string, long> paidData = GetAddressAndAmountFromFiles(paidFilePaths);
                foreach (string data in keys)
                {
                    if (paidData.ContainsKey(data))
                    {
                        LogHelper.Info($"address is {data} before delete paid amount is {dic[data]}");
                        dic[data] -= paidData[data];
                        LogHelper.Info($"address is {data} after delete paid amount is {dic[data]}");
                    }
                }
                LogHelper.Info($"=================================end to delete paid amount==============================================");
                /*
                LogHelper.Info($"=================================begin to add unpaid amount==============================================");
                //添加未发放的
                Dictionary<string, long> unpaidData = GetAddressAndAmountFromFiles(unpaidFilePath);
                foreach (string data in keys)
                {
                    if (unpaidData.ContainsKey(data))
                    {
                        LogHelper.Info($"address is {data} before add unpaid amount is {dic[data]}");
                        dic[data] -= unpaidData[data];
                        LogHelper.Info($"address is {data} after add unpaid amount is {dic[data]}");
                    }
                }
                LogHelper.Info($"=================================end to add unpaid amount==============================================");

                //遍历输出所有的
                long sumAmount = 0;
                foreach (var data in dic)
                {
                    sumAmount += data.Value;
                    LogHelper.Info($"address is {data.Key} actual send reward amount is {data.Value}");
                }
                //更新Miners表，这个稍后添加
                LogHelper.Info($"sum amount value is {sumAmount / 100000000}");
                */
                long sumAmount = 0;
                List<SendRawTransactionOutputsIM> allReceiversList = new List<SendRawTransactionOutputsIM>();
                foreach (var data in dic)
                {
                    sumAmount += data.Value;
                    allReceiversList.Add(new SendRawTransactionOutputsIM { Address = data.Key, Amount = data.Value });
                }
                //开始组织SendRawTransaction数据
                List<List<SendRawTransactionOutputsIM>> listGroup = new List<List<SendRawTransactionOutputsIM>>();
                for (int i = 0; i < allReceiversList.Count; i += 50)
                {
                    listGroup.Add(allReceiversList.Skip(i).Take(50).ToList());
                }
                //分页发放奖励
                foreach (List<SendRawTransactionOutputsIM> receiverList in listGroup)
                {
                    //调用ListPageUnspent接口
                    AuthenticationHeaderValue authHeaderValue = null;
                    RpcClient client = new RpcClient(new Uri("http://127.0.0.1:5007/"), authHeaderValue, null, null, "application/json");
                    RpcRequest transactionRequest = RpcRequest.WithParameterList("ListPageUnspent", new List<object> { 1, 1, 3000, 9999999, 1, 9999999999999999, true }, 1);
                    RpcResponse transactionResponse = await client.SendRequestAsync(transactionRequest);
                    if (transactionResponse.HasError)
                    {
                        LogHelper.Error(transactionResponse.Error.Message.ToString());
                        throw new Exception(transactionResponse.Error.Message.ToString());
                    }
                    ListPageUnspentOM unspent = transactionResponse.GetResult<ListPageUnspentOM>();

                    //组装Sender 
                    long totalAmount = 0;
                    long totalItemActualAmount = receiverList.Sum(p => p.Amount);
                    List<SendRawTransactionInputsIM> sendersList = new List<SendRawTransactionInputsIM>();
                    //类型转为SendRawTransactionOutputsIM

                    foreach (var senderItem in unspent.UnspentOMList)
                    {
                        //判断总的输出金额和
                        if (totalAmount + 3 < (totalItemActualAmount))
                        {
                            if (senderItem.spendable == false && senderItem.confirmations > 100)
                            {
                                LogHelper.Info($"avaliable txhash is {senderItem.txid}, avaliable vout is {senderItem.vout}");
                                sendersList.Add(new SendRawTransactionInputsIM { TxId = senderItem.txid, Vout = Convert.ToInt32(senderItem.vout) });
                                totalAmount += senderItem.amount;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    foreach (var receiver in receiverList)
                    {
                        LogHelper.Info($"send reward to {receiver.Address}, send amount is {receiver.Amount}");
                    }
                    //调用接口SendRawTransaction，发送交易
                    string txHash = "";
                    try
                    {
                        //先解密
                        RpcRequest passphraseRequest = RpcRequest.WithParameterList("WalletPassphrase", new[] { password }, 1);
                        RpcResponse passphraseResponse = await client.SendRequestAsync(passphraseRequest);
                        if (passphraseResponse.HasError)
                        {
                            LogHelper.Error(passphraseResponse.Error.Message.ToString());
                            throw new Exception(passphraseResponse.Error.Message.ToString());
                        }

                        RpcRequest request = RpcRequest.WithParameterList("SendRawTransaction", new List<object> { sendersList.ToArray(), receiverList.ToArray(), "fiiimZE5PDLtXXBT1i54UTqGU3bkoYVyAFUbPY", 0, 1024 }, 1);
                        RpcResponse response = await client.SendRequestAsync(request);
                        if (response.HasError)
                        {
                            LogHelper.Error(response.Error.Message.ToString());
                            throw new Exception(response.Error.Message.ToString());
                        }
                        txHash = response.GetResult<string>();
                    }
                    catch (Exception exc)
                    {
                        LogHelper.Error(exc.ToString());
                        if (exc.Message.Contains("1010001"))
                        {
                            return;
                        }
                        if (sendersList.Count <= 0)
                        {
                            return;
                        }
                        System.Threading.Thread.Sleep(60000);

                        RpcRequest request = RpcRequest.WithParameterList("GetTxHashByInput", new List<object> { sendersList.First().TxId, sendersList.First().Vout }, 1);
                        RpcResponse response = await client.SendRequestAsync(request);
                        if (response.HasError)
                        {
                            LogHelper.Error(response.Error.Message.ToString());
                            throw new Exception(response.Error.Message.ToString());
                        }
                        txHash = response.GetResult<string>();
                        if (string.IsNullOrEmpty(txHash))
                        {
                            return;
                        }
                    }
                    //记录一下发放日志：地址，金额，期数，锁定时间，交易Hash
                    LogHelper.Info($"this transaction hash is: {txHash}");
                    //更新数据库状态
                    foreach (SendRawTransactionOutputsIM point in receiverList)
                    {
                        LogHelper.Info($"begin to update reward list the address is {point.Address}, txHash is {txHash}");
                        //更新tableName中的对应地址，对应txid中的txhash
                        foreach (string table in tableName)
                        {
                            using (MySqlConnection conn = new MySqlConnection(CacheConnectionString))
                            {
                                string sql = $"Update {table} SET TransactionHash = '{txHash}' WHERE MinerAddress = '{point.Address}' AND TransactionHash IN('EF771498206FE2BE9BFF5C5F22943FDDB3F74370605E7E2584DA6C43FD15B8CF', '1B25B3031A6F5227ED8A650F70045CCD67B309E24196A2CFEE860B8B7A7E39D4')";
                                LogHelper.Info($"Update {table} SET TransactionHash = '{txHash}' WHERE MinerAddress = '{point.Address}' AND TransactionHash IN('EF771498206FE2BE9BFF5C5F22943FDDB3F74370605E7E2584DA6C43FD15B8CF', '1B25B3031A6F5227ED8A650F70045CCD67B309E24196A2CFEE860B8B7A7E39D4')");
                                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                                {
                                    cmd.Connection.Open();
                                    cmd.CommandTimeout = 1200;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        private Dictionary<string, long> GetAddressAndAmountFromFiles(List<string> filePaths)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();
            foreach (var filePath in filePaths)
            {
                //文件中读取address和amount
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line = null;
                        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                        {
                            string address = line.Split(',')[0];
                            long amount = Convert.ToInt64(line.Split(',')[1]);
                            if (dic.ContainsKey(address))
                            {
                                dic[address] += amount;
                            }
                            else
                            {
                                dic.Add(address, amount);
                            }
                        }
                    }
                }
            }
            return dic;
        }

        private Dictionary<string, long> GetAddressAndAmountFromFile(string filePath)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();
            //文件中读取address和amount
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line = null;
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                    {
                        string address = line.Split(',')[0];
                        long amount = Convert.ToInt64(line.Split(',')[1]);
                        if (dic.ContainsKey(address))
                        {
                            dic[address] += amount;
                        }
                        else
                        {
                            dic.Add(address, amount);
                        }
                    }
                }
            }
            return dic;
        }
    }
}
