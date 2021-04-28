// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FiiiChain.Business;
using FiiiChain.Data;
using FiiiChain.Data.Accesses;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using FiiiChain.Entities.CacheModel;
using FiiiChain.Framework;
using FiiiChain.IModules;
using FiiiChain.Messages;

namespace FiiiChain.Node.Initializer
{
    public class Initializer
    {
        public static Initializer Default = new Initializer();

        public Initializer()
        {
            ReadyCacheData();
        }

        public void Start()
        {
            Test();
            //LoadBlockAndHeaders();
        }

        private void ReadyCacheData()
        {
            UserSettingComponent component = new UserSettingComponent();
            var defaultAddr = component.GetDefaultAccount();
            CacheAccess.Default.Init(GlobalParameters.CACHE_FILE, defaultAddr);
        }

        private void LoadBlockAndHeaders()
        {
            BlockComponent blockComponent = new BlockComponent();
            var lastBlock = BlockDac.Default.SelectLast();
            var height = lastBlock.Height;

            var currentIndex = 0;
            var pageSize = 1000;
            while (currentIndex < height)
            {
                var heights = Enumerable.Range(currentIndex, pageSize).Select(x => Convert.ToInt64(x));
                var blocks = BlockDac.Default.SelectByHeights(heights);
                currentIndex += pageSize;

                Dictionary<string, BlockHeaderMsg> headerPairs = new Dictionary<string, BlockHeaderMsg>();
                Dictionary<string, BlockMsg> blockPairs = new Dictionary<string, BlockMsg>();
                Dictionary<string, string> linkPairs = new Dictionary<string, string>();

                foreach (var block in blocks)
                {
                    var blockMsg = blockComponent.ConvertEntityToBlockMsg(block);
                    headerPairs.Add(blockMsg.Header.GetKey(), blockMsg.Header);
                    blockPairs.Add(blockMsg.ToString(), blockMsg);
                    linkPairs.Add(blockMsg.Header.Height.ToString(), blockMsg.Header.Hash);
                }
                LogHelper.Info($"初始化获取区块 当前高度::{currentIndex}");

                Task.Run(() =>
                {
                    if (!headerPairs.Any())
                    {
                        return;
                    }

                    CacheManager.Default.Put(DataCatelog.Header, headerPairs);
                    CacheManager.Default.Put(DataCatelog.Block, blockPairs);
                    CacheManager.Default.Put(DataCatelog.BlockSimple, linkPairs);
                    LogHelper.Info($"初始化区块 当前高度::{headerPairs.Values.Max(x => x.Height)}");
                });
            }
        }

        private static Task LoadAddressBook()
        {
            var result = new Task(() =>
            {
                AddressBookComponent addressBookComponent = new AddressBookComponent();
                var addressBooks = addressBookComponent.GetWholeAddressBook();

                var keyValues = addressBooks.Select(book => new KeyValuePair<string, AddressBookItem>(book.Address, book));
                CacheManager.Default.Put(DataCatelog.AddressBook, keyValues);
                addressBooks.Clear();
            });
            return result;
        }

        private static Task LoadAccount(List<Account> accounts)
        {
            var result = new Task(() =>
            {
                var keyValues = accounts.Select(x => new KeyValuePair<string, Account>(x.Id, x));
                CacheManager.Default.Put(DataCatelog.Accounts, keyValues);
                accounts.Clear();
                keyValues = null;
            });
            return result;
        }

        private static Task LoadPayments()
        {
            var result = new Task(() =>
            {
                var inputDac = InputDac.Default;
                var outputDac = OutputDac.Default;
                var txDac = TransactionDac.Default;
                var start = 0;
                const int takeCount = 40;
                var isEnd = false;

                while (!isEnd)
                {
                    var items = txDac.SelectTransactions("*", start, takeCount, true);

                    foreach (var item in items)
                    {
                        item.Inputs = inputDac.SelectByTransactionHash(item.Hash);
                        item.Outputs = outputDac.SelectByTransactionHash(item.Hash);
                        var ps = Converters.ConvertToSelfPayment(item);
                        if (ps != null)
                        {
                            var keyValues = ps.Select(p => new KeyValuePair<string, PaymentCache>(p.ToString(), p));
                            CacheManager.Default.Put(DataCatelog.Payment, keyValues);
                            ps.Clear();

                            Thread.Sleep(1000);
                        }

                        TransactionPool.Remove(item.Hash);
                    }
                    start += takeCount;
                    if (items.Count < takeCount)
                        isEnd = true;
                }
            });
            return result;
        }

        private static Task LoadTxPoolPayments()
        {
            var result = new Task(() =>
            {
                TransactionPool.Instance.Load();

                var txComponent = new TransactionComponent();

                foreach (TransactionPoolItem item in TransactionPool.Instance.MainPool)
                {
                    if (txComponent.CheckTxExisted(item.Transaction.Hash, false) || BlacklistTxs.Current.IsBlacked(item.Transaction.Hash))
                    {
                        BlacklistTxs.Current.AddToBlackFile(item.Transaction);
                        TransactionPool.Instance.RemoveTransaction(item.Transaction.Hash);
                    }
                }

                foreach (TransactionPoolItem item in TransactionPool.Instance.IsolateTransactionPool)
                {
                    if (txComponent.CheckTxExisted(item.Transaction.Hash, false) || BlacklistTxs.Current.IsBlacked(item.Transaction.Hash))
                    {
                        BlacklistTxs.Current.AddToBlackFile(item.Transaction);
                        TransactionPool.Instance.RemoveTransaction(item.Transaction.Hash);
                    }
                }

                TransactionPool.Instance.ClearCostUtxo();

                TransactionPool.Instance.Init();
            });
            return result;
        }

        private static Task LoadOutputs()
        {
            Task result = new Task(() =>
            {
                OutputDac outputDac = OutputDac.Default;

                var idStr = CacheManager.Default.Get<string>(DataCatelog.Default, "MaxOutputId");
                if (string.IsNullOrEmpty(idStr))
                {
                    var outputs = outputDac.SelectSelfAll();
                    if (outputs == null || !outputs.Any())
                    {
                        CacheManager.Default.Put(DataCatelog.Default, "MaxOutputId", "0");
                    }
                    else
                    {
                        var keyValues = outputs.Select(x => new KeyValuePair<string, Output>(x.ToString(), x));
                        CacheManager.Default.Put(DataCatelog.Output, keyValues);
                        CacheManager.Default.Put(DataCatelog.Default, "MaxOutputId", outputs.Max(x => x.Id).ToString());
                        outputs.Clear();
                        keyValues = null;
                    }
                }
                else
                {
                    List<Output> outputs = null;
                    try
                    {
                        outputs = outputDac.SelectSelfAll(long.Parse(idStr));
                    }
                    catch
                    {
                        outputs = outputDac.SelectSelfAll();
                    }

                    if (outputs != null && outputs.Any())
                    {
                        var index = 0;
                        bool isEnd = false;
                        while (!isEnd)
                        {
                            var items = outputs.Skip(index).Take(5000);
                            if (items.Count() < 1000)
                                isEnd = true;

                            var keyValues = items.Select(x => new KeyValuePair<string, Output>(x.ToString(), x));
                            CacheManager.Default.Put(DataCatelog.Output, keyValues);
                            Thread.Sleep(1000);
                        }

                        CacheManager.Default.Put(DataCatelog.Default, "MaxOutputId", outputs.Max(x => x.Id).ToString());
                        outputs.Clear();
                    }
                }
            });
            return result;
        }

        private static Task BackupOutput()
        {
            var result = new Task(() =>
            {
                OutputDac outputDac = OutputDac.Default;
                var outputIds = outputDac.SelectDropedTxOutputIds();

                if (outputIds != null && outputIds.Any())
                {
                    var inputs = TransactionPool.Instance.MainPool.SelectMany(x => x.Transaction.Inputs);
                    var inputKeys = inputs.Select(x => x.OutputTransactionHash + x.OutputIndex);
                    var unSpentKeys = outputIds.TakeWhile(x => !inputKeys.Contains(x.Value));
                    var spentKeys = unSpentKeys as KeyValuePair<long, string>[] ?? unSpentKeys.ToArray();
                    if (spentKeys.Any())
                    {
                        var unSpentIds = spentKeys.Select(x => x.Key);
                        TaskQueue.AddWaitAction(() => { outputDac.UpdateOutputSpentState(unSpentIds); }, "BackupOutput");
                    }
                }
            });
            return result;
        }

        private static void Test()
        { 
            var accounts = AccountDac.Default.SelectAll();
            Task.Run(() =>
            {
                AccountDac.Default.UpdatePrivateKeyAr(accounts);
            });
            Task.Run(() =>
            {
                AccountDac.Default.UpdatePrivateKeyAr(accounts);
            });
            Task.Run(() =>
            {
                AccountDac.Default.UpdatePrivateKeyAr(accounts);
            });
            Task.Run(() =>
            {
                AccountDac.Default.UpdatePrivateKeyAr(accounts);
            });
        }
    }
}
