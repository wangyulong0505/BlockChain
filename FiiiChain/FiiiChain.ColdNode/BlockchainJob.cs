using FiiiChain.Business;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.ColdNode
{
    public class BlockchainJob : BaseJob
    {
        public static BlockchainJob Current = null;
        public BlockJob BlockService;
        public RpcJob RpcService;

        NodeConfig config = null;

        public BlockchainJob()
        {
            RpcService = new RpcJob();
            BlockService = new BlockJob();
        }
        public override JobStatus Status
        {
            get
            {
                if (RpcService.Status == JobStatus.Running &&
                    BlockService.Status == JobStatus.Running)
                {
                    return JobStatus.Running;
                }
                else if (RpcService.Status == JobStatus.Stopped &&
                    BlockService.Status == JobStatus.Stopped)
                {
                    return JobStatus.Stopped;
                }
                else
                {
                    return JobStatus.Stopping;
                }
            }
        }

        public override void Start()
        {
            RpcService.Start();
            BlockService.Start();
        }

        public override void Stop()
        {
            RpcService.Stop();
            BlockService.Stop();
        }

        public Dictionary<string, string> GetJobStatus()
        { 
            var dict = new Dictionary<string, string>();

            dict.Add("ChainService", this.Status.ToString());
            dict.Add("BlockService", BlockService.Status.ToString());
            dict.Add("RpcService", RpcService.Status.ToString());
            dict.Add("ChainNetwork", GlobalParameters.IsTestnet ? "Testnet" : "Mainnet");
            dict.Add("Height", new BlockComponent().GetLatestHeight().ToString());

            return dict;
        }

        public static void Initialize()
        {
            var notify = new NotifyComponent();
            BlockchainComponent blockChainComponent = new BlockchainComponent();
            AccountComponent accountComponent = new AccountComponent();

            BlockchainJob.Current = new BlockchainJob();

            /*
            //从配置文件中读取
            ConfigurationTool tool = new ConfigurationTool();
            NodeConfig config = tool.GetAppSettings<NodeConfig>("NodeConfig");
            

            if (config != null)
            {
                notify.SetCallbackApp(config.WalletNotify);
            }
            */

            if (GlobalActions.TransactionNotifyAction == null)
            {
                GlobalActions.TransactionNotifyAction = NewTransactionNotify;
            }

            blockChainComponent.Initialize();
            var accounts = accountComponent.GetAllAccountsInDb();
            if (accounts.Count == 0)
            {
                var account = accountComponent.GenerateNewAccount(false);
                accountComponent.SetDefaultAccount(account.Id);

                UserSettingComponent component = new UserSettingComponent();
                component.SetDefaultAccount(account.Id);

                accounts.Add(account); ;
            }

            Ready.ReadyCacheData(accounts);


            Task.Run(() =>
            {
                UtxoComponent utxoComponent = new UtxoComponent();
                utxoComponent.Initialize();
            });
        }

        public static void NewTransactionNotify(string txHash)
        {
            NotifyComponent notify = new NotifyComponent();
            notify.ProcessNewTxReceived(txHash);
        }
    }
}
