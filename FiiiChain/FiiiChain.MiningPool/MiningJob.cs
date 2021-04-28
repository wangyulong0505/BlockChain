using FiiiChain.Consensus;
using FiiiChain.Entities;
using FiiiChain.Framework;
using FiiiChain.MiningPool.Agent;
using FiiiChain.MiningPool.API;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using FiiiChain.MiningPool.Rules;
using FiiiChain.PoolMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace FiiiChain.MiningPool
{
    class MiningJob
    {
        SocketServer server;
        WalletAPI api;
        string minerName;
        string walletAddress;
        byte[] privateKey;

        const long MaxNonceCount = 262144; //64GB

        long currentHeight;
        long currentBaseTarget;
        int currentScoopNumber;
        long miningStartTime;
        long lastScoopDataTime;
        Messages.BlockMsg currentBlock;
        bool isStarting = false;

        SafeCollection<Miner> MinerList;
        Dictionary<string, long> minerEffort;
        Dictionary<string, long> minerEffort_OneStartTask;
        long totalEffort;
        long updateloadCount = 0;
        long baseTarget;
        long generatedBlockCount = 0;

        Timer timer;
        Timer updateBlocksTimer;
        Timer uploadHashsTimer;
        Timer updateBlockRateTimer;

        private MiningStatus MiningStatus =  MiningStatus.Wait;
        Timer blockAutoGenerationTimer;
        Dictionary<long, Messages.BlockMsg> pendingBlocks = new Dictionary<long, Messages.BlockMsg>();
        Dictionary<string, long> minerLastDataTime = new Dictionary<string, long>();
        int MiningTimes = 0;
        StackExchange.Redis.ConnectionMultiplexer redisConn;
        StackExchange.Redis.IDatabase redisDb;

        public bool IsRunning { get; set; }
        public MiningJob()
        {
            Startup.MinerListAction = GetMinerList;
            MinerList = new SafeCollection<Miner>();
            this.minerEffort = new Dictionary<string, long>();
            this.minerEffort_OneStartTask = new Dictionary<string, long>();
            server = new SocketServer(10000, Int16.MaxValue);
            var uri = MiningPoolSetting.API_URI;
            api = new WalletAPI(uri);
        }

        public List<string> GetMinerList()
        {
            List<string> result = new List<string>();
            if (MinerList == null)
                return result;
            result.Add(string.Format("CurrentBlockHeight:{0}", currentBlock.Header.Height));
            result.Add(string.Format("MiningTimes:{0}", this.MiningTimes));
            result.Add(string.Format("MinintStartTime:{0}", Time.GetLocalDateTime(this.miningStartTime)));
            result.Add(string.Format("MinersCount:{0}", MinerList.Count));
            foreach (Miner item in MinerList)
            {
                try
                {
                    long effort = 0;
                    if (minerEffort.ContainsKey(item.WalletAddress))
                        effort = minerEffort[item.WalletAddress];
                    if (minerEffort_OneStartTask.ContainsKey(item.WalletAddress))
                        effort = minerEffort_OneStartTask[item.WalletAddress];
                    var str = string.Format("IP:{0}|Address:{1}|Effort:{2}", item.ClientAddress, item.WalletAddress, effort);
                    result.Add(str);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message, ex);
                }
            }
            return result;
        }


        public void Init(string minerName, string walletAddress, string password = null)
        {
            if(api.GetTxSettings().Encrypt)
            {
                Console.Write("Please enter your wallet password:");
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
                    Console.WriteLine("password can not be empty");
                    return;
                }
            }

            server.ReceivedCommandAction = receivedCommand;
            server.ReceivedMinerConnectionAction = receivedConnection;
            this.minerName = minerName;
            this.walletAddress = walletAddress;
            try
            {
                this.privateKey = api.DumpPrivateKey(walletAddress, password);
                if (privateKey == null)
                {
                    LogHelper.Error("Address does not exist");
                    return;
                }
            }
            catch
            {
                throw new Exception("Please wait Node service startup");
            }
            
            timer = new Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;

            blockAutoGenerationTimer = new Timer(60 * 1000);
            blockAutoGenerationTimer.AutoReset = true;
            blockAutoGenerationTimer.Elapsed += BlockAutoGenerationTimer_Elapsed;

            const int updateBlocksTime = 1000 * 60;//1 Min
            updateBlocksTimer = new Timer(updateBlocksTime);
            updateBlocksTimer.AutoReset = true;
            updateBlocksTimer.Elapsed += updateBlocksTimer_Elapsed;

            const int uploadHashsTime = 10 * 1000 * 60;//10 Min
            uploadHashsTimer = new Timer(uploadHashsTime);
            uploadHashsTimer.AutoReset = true;
            uploadHashsTimer.Elapsed += uploadHashsTimer_Elapsed;

            const int updateBlockRateTime = 1000 * 60 * 60; //1 Hour
            updateBlockRateTimer = new Timer(updateBlockRateTime);
            updateBlockRateTimer.AutoReset = true;
            updateBlockRateTimer.Elapsed += updateBlockRateTimer_Elapsed;

            redisConn = StackExchange.Redis.ConnectionMultiplexer.Connect("r-3ns10cea37457624.redis.rds.aliyuncs.com,abortConnect=False");
            redisDb = redisConn.GetDatabase(5);
        }

        private void BlockAutoGenerationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void receivedCommand(TcpReceiveState state, PoolCommand cmd)
        {
            switch (cmd.CommandName)
            {
                case CommandNames.Login:
                    this.receivedLoginCommand(state, cmd);
                    break;
                case CommandNames.NonceData:
                    this.receivedNonceDataCommand(state, cmd);
                    break;
                case CommandNames.ScoopData:
                    this.receivedScoopDataCommand(state, cmd);
                    break;
                case CommandNames.Heartbeat:
                    this.receivedHeartbeatCommand(state, cmd);
                    break;
                default:
                    break;
            }
        }

        private void updateBlocksTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                BlocksAPI.Current.GetVerifiedHashes();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in block data verification", ex);
            }
        }
        
        private void uploadHashsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                HashRatesComponent hashRatesComponent = new HashRatesComponent();
                var hashCount = this.totalEffort - updateloadCount;
                hashRatesComponent.SaveHashRates(hashCount);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void updateBlockRateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            long difficulty = Convert.ToInt64(POC.CalculateDifficulty(currentBlock.Header.Bits));
            BlocksAPI.Current.SaveBlockRates(generatedBlockCount, difficulty);
            generatedBlockCount = 0;
        }

        private bool receivedConnection(TcpState e, bool connected)
        {
            try
            {
                var miner = this.MinerList.Where(m => m.ClientAddress == e.ClientAddress.ToString()).FirstOrDefault();

                if (connected)
                {
                    if (miner != null && miner.IsConnected)
                    {
                        this.sendRejectCommand(e);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (miner != null)
                    {
                        miner.IsConnected = false;
                        this.MinerList.Remove(miner);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }

            return false;
        }
        
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //LogHelper.Warn($"Current status: {this.MiningStatus}, {this.MinerList.Any(m => m.IsConnected)}, {!isStarting}");
            //timer.Enabled = false;
            if (this.MiningStatus == MiningStatus.Saving)
                return;

            if (this.MiningStatus == MiningStatus.Wait && this.MinerList.Any(m => m.IsConnected) && !isStarting)
            {
                AddEffort2MinerEffort();
                this.minerEffort_OneStartTask.Clear();
                this.startMining();
            }
            else if (this.MiningStatus == MiningStatus.Mining)
            {
                if (Time.EpochTime - lastScoopDataTime > 10 * 1000/* || Time.EpochTime - this.miningStartTime > 15 * 60 * 1000*/)
                {
                    LogHelper.Warn("Not received scoop data for a long time, mining task is auto stopped.");
                    this.stopMining(false, MiningStatus.Wait);
                }
                else
                {
                    if(MiningPoolSetting.PoolType != "Server")
                    {
                        var dictEffort = new Dictionary<string, long>();

                        foreach(var key in minerEffort.Keys)
                        {
                            dictEffort[key] = minerEffort[key];
                        }

                        foreach (var key in minerEffort_OneStartTask.Keys)
                        {                         
                            if(dictEffort.ContainsKey(key))
                            {
                                dictEffort[key] += minerEffort_OneStartTask[key];
                            }
                            else
                            {
                                dictEffort[key] = minerEffort_OneStartTask[key];
                            }
                        }

                        var data = JsonConvert.SerializeObject(dictEffort);
                        var redisKey = "MiningPool:MinerEffor";

                        if(GlobalParameters.IsTestnet)
                        {
                            redisKey = "MiningPool:Testnet:MinerEffor";
                        }

                        redisDb.StringSet(redisKey, data);
                    }
                }
            }
            //timer.Enabled = true;
        }

        public void Start(IPEndPoint ep)
        {
            this.MinerList.Clear();
            this.minerEffort.Clear();
            this.minerEffort_OneStartTask.Clear();
            this.server.Start(ep);
            var result = api.GetBlockChainInfo();

            if (!result.isRunning)
            {
                throw new Exception("Please wait Node service startup");
            }
            else if (result.connections < 2)
            {
                throw new Exception("Node Server must more than 2");
            }

            if(result.localLastBlockHeight < result.remoteLatestBlockHeight)
            {
                throw new Exception("Please wait block data synchronize finished");
            }

            this.IsRunning = true;
            this.timer.Start();
            this.updateBlocksTimer.Start();
            this.uploadHashsTimer.Start();
            this.updateBlockRateTimer.Start();
            LogHelper.Info("Server started");
            Console.ReadLine();
        }

        public void Stop()
        {
            this.IsRunning = false;
            this.timer.Stop();
            this.updateBlocksTimer.Stop();
            this.uploadHashsTimer.Stop();
            this.updateBlockRateTimer.Stop();
            this.stopMining(false, MiningStatus.Wait);
        }

        private void startMining()
        {
            isStarting = true;
            this.miningStartTime = Time.EpochTime;
            this.MiningTimes++;

            LogHelper.Info($"startMining(), times:{this.MiningTimes}");
            try
            {
                this.currentBlock = api.GenerateNewBlock(minerName, walletAddress);

                if (MiningPoolSetting.PoolType != "Server" && this.currentBlock.Header.Height > this.currentHeight)
                {
                    this.minerEffort.Clear();
                    this.totalEffort = 0;
                    this.MiningTimes = 0;
                }

                this.currentHeight = this.currentBlock.Header.Height;
                LogHelper.Info($"GenerateNewBlock, currentHeight:{this.currentHeight}");
                //this.currentBaseTarget = api.GetBaseTarget(this.currentHeight);
                this.lastScoopDataTime = Time.EpochTime;
                this.MiningStatus = MiningStatus.Mining;
                ScoopDataRule.Current.Clear();
                Messages.BlockMsg lastBlock = null;

                if (this.currentHeight > 0)
                {
                    lastBlock = api.GetBlockByHeight(this.currentHeight - 1);

                    Messages.BlockMsg prevStepBlock = null;
                    if (this.currentHeight >= POC.DIFFIUCLTY_ADJUST_STEP)
                    {
                        if (!GlobalParameters.IsTestnet && this.currentHeight <= POC.DIFFICULTY_CALCULATE_LOGIC_ADJUST_HEIGHT)
                        {
                            prevStepBlock = api.GetBlockByHeight(this.currentHeight - POC.DIFFIUCLTY_ADJUST_STEP - 1);
                        }
                        else
                        {
                            prevStepBlock = api.GetBlockByHeight(this.currentHeight - POC.DIFFIUCLTY_ADJUST_STEP);
                        }
                    }

                    var lastBlockEntity = new Block()
                    {
                        Hash = lastBlock.Header.Hash,
                        Height = lastBlock.Header.Height,
                        Bits = lastBlock.Header.Bits,
                        Timestamp = lastBlock.Header.Timestamp
                    };
                    Block prevStepBlockEntity = null;

                    if (prevStepBlock != null)
                    {
                        prevStepBlockEntity = new Block()
                        {
                            Hash = prevStepBlock.Header.Hash,
                            Height = prevStepBlock.Header.Height,
                            Bits = prevStepBlock.Header.Bits,
                            Timestamp = prevStepBlock.Header.Timestamp
                        };
                    }

                    LogHelper.Info($"process 1");
                    try
                    {
                        baseTarget = POC.CalculateBaseTarget(currentHeight, lastBlockEntity, prevStepBlockEntity);
                        LogHelper.Info($"Basetarget is {baseTarget}");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.ToString());
                    }
                }
                else
                {
                    baseTarget = POC.CalculateBaseTarget(0, null, null);
                }

                this.currentBaseTarget = baseTarget;
                this.currentScoopNumber = POC.GetScoopNumber(this.currentBlock.Header.PayloadHash, this.currentHeight);
                LogHelper.Info($"currentScoopNumber is {currentScoopNumber}");

                LogHelper.Info("Start mining block " + this.currentBlock.Header.Height);

                StartMsg msg = new StartMsg();
                msg.BlockHeight = this.currentHeight;
                msg.ScoopNumber = this.currentScoopNumber;
                msg.StartTime = this.miningStartTime;
                msg.GenHash = GenHash(currentBlock.Header.PayloadHash, currentBlock.Header.Height);

                var startCmd = PoolCommand.CreateCommand(CommandNames.Start, msg);

                var removeMiners = new List<Miner>();
                Parallel.ForEach(MinerList, miner => {
                    //foreach (Miner miner in this.MinerList)
                    //{
                    if (!miner.IsConnected)
                    {
                        removeMiners.Add(miner);
                    }
                    else
                    {
                        try
                        {
                            this.sendStartCommand(new TcpState { Client = miner.Client, Stream = miner.Stream, ClientAddress = miner.ClientAddress, Data = startCmd }, startCmd);
                        }
                        catch
                        {
                            removeMiners.Add(miner);
                        }
                    }
                    //}
                });

                MinerList.RemoveAll(x => removeMiners.Contains(x));
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in startMining", ex);
            }
        }

        private void stopMining(bool result, MiningStatus status)
        {
            isStarting = false;
            this.MiningStatus = status;
            StopMsg stopMsg = new StopMsg();
            stopMsg.Result = result;
            stopMsg.BlockHeight = this.currentHeight;
            stopMsg.StartTime = this.miningStartTime;
            stopMsg.StopTime = Time.EpochTime;

            var stopCmd = PoolCommand.CreateCommand(CommandNames.Stop, stopMsg);

            List<Miner> removeMiners = new List<Miner>();

            Parallel.ForEach(MinerList, miner =>
            {
            //foreach (Miner miner in this.MinerList)
            //{
                if (!miner.IsConnected)
                {
                    removeMiners.Add(miner);
                }
                else
                { 
                    try
                    {
                        this.sendStopCommand(new TcpState { Client = miner.Client, Stream = miner.Stream, ClientAddress = miner.ClientAddress, Data = stopCmd }, stopCmd);
                    }
                    catch
                    {
                        removeMiners.Add(miner);
                    }
                }
                //}
            });

            MinerList.RemoveAll(x => removeMiners.Contains(x));
            if (result)
            {
                LogHelper.Info("Block height " + this.currentHeight + " is generated success");
            }
            else
            {
                LogHelper.Info("Block height " + this.currentHeight + " is generated failed");
            }
        }

        private bool forgeBlock(string minerAddress, long nonce,byte[] targetBytes)
        {
            try
            {
                //区块由矿池生成，奖励由矿池分配给矿工
                this.currentBlock.Header.GeneratorId = minerAddress;
                this.currentBlock.Header.Nonce = nonce;
                this.currentBlock.Header.Timestamp = Time.EpochTime;

                var dsa = ECDsa.ImportPrivateKey(this.privateKey);
                    
                this.currentBlock.Header.BlockSignature = Base16.Encode(dsa.SingnData(Base16.Decode(this.currentBlock.Header.PayloadHash)));

                this.currentBlock.Header.BlockSigSize = this.currentBlock.Header.BlockSignature.Length;
                this.currentBlock.Header.Hash = this.currentBlock.Header.GetHash();
                this.currentBlock.Header.TotalTransaction = this.currentBlock.Transactions.Count;

                LogHelper.Info("forgeBlock start to GetMiningWorkResult");
                var hashResult = this.GetMiningWorkResult(currentBlock);

                if (POC.Verify(currentBlock.Header.Bits, hashResult))
                {
                    LogHelper.Info($"forgeBlock POC.Verify is true");
                    this.MiningStatus = MiningStatus.Saving;
                    try
                    {
                        LogHelper.Warn(currentBlock.Header.ToString());
                        api.SubmitBlock(this.currentBlock);
                        this.stopMining(true, MiningStatus.Saving);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.ToString());

                        try
                        {
                            var nodeHeight = api.GetBlockCount();

                            if(nodeHeight == this.currentHeight)
                            {
                                this.stopMining(true, MiningStatus.Saving);
                                return true;
                            }
                            else
                            {
                                this.lastScoopDataTime = Time.EpochTime;
                                this.MiningStatus = MiningStatus.Mining;
                                return false;
                            }
                        }
                        catch
                        {
                            this.lastScoopDataTime = Time.EpochTime;
                            this.MiningStatus = MiningStatus.Mining;
                            return false;
                        }
                    }
                }
                else
                {
                    LogHelper.Info($"forgeBlock POC.Verify is false");
                }
                return false;
            }
            catch(Exception e)
            {
                LogHelper.Error("Submit block failed: " + e.Message, e);
                return false;
            }
        }

        public bool Verify(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;
            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
        }


        private byte[] GetMiningWorkResult(Messages.BlockMsg block)
        {
            var listBytes = new List<Byte>();
            listBytes.AddRange(Base16.Decode(block.Header.PayloadHash));
            listBytes.AddRange(BitConverter.GetBytes(block.Header.Height));
            var genHash = Sha3Helper.Hash(listBytes.ToArray());
            var scoopNumber = POC.GetScoopNumber(block.Header.PayloadHash, block.Header.Height);
            var scoopData = POC.CalculateScoopData(block.Header.GeneratorId, block.Header.Nonce, scoopNumber);
            List<byte> targetByteLists = new List<byte>();
            targetByteLists.AddRange(scoopData);
            targetByteLists.AddRange(genHash);
            var baseTarget = Sha3Helper.Hash(targetByteLists.ToArray());
            return baseTarget;
        }

        private void receivedRegistCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var registMsg = new RegistMsg();
            int index = 0;
            registMsg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.FirstOrDefault(m => m.WalletAddress == registMsg.WalletAddress || m.SerialNo == registMsg.SerialNo);
            
            if (miner != null || MinersAPI.Current.POSValidate(registMsg.WalletAddress, registMsg.SerialNo))
            {
                this.sendRejectCommand(e);
            }
            else
            {
                var miners = MinersAPI.Current.SaveMiners(registMsg.WalletAddress, registMsg.Name, registMsg.SerialNo);

                var result = miners != null;
                this.sendRegistResultCommand(e, result);
            }
        }

        private void receivedLoginCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var loginMsg = new LoginMsg();
            int index = 0;
            loginMsg.Deserialize(cmd.Payload, ref index);
            
            if (!MinersAPI.Current.POSValidate(loginMsg.WalletAddress, loginMsg.SerialNo))
            {
                this.sendRejectCommand(e);
                return;
            }
            
            //TODO: address and SerialNo and account only for one Minner
            var miner = this.MinerList.FirstOrDefault(m => m.WalletAddress == loginMsg.WalletAddress || m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString() || m.SerialNo == loginMsg.SerialNo);
            if (miner != null)
            {
                StopMsg stopMsg = new StopMsg();
                stopMsg.Result = false;
                stopMsg.BlockHeight = this.currentHeight;
                stopMsg.StartTime = this.miningStartTime;
                stopMsg.StopTime = Time.EpochTime;

                var stopCmd = PoolCommand.CreateCommand(CommandNames.Stop, stopMsg);
                this.sendStopCommand(new TcpState { Client = miner.Client, Stream = miner.Stream, ClientAddress = miner.ClientAddress, Data = stopCmd }, stopCmd);
                MinerList.Remove(miner);
            }

            miner = new Miner();
            miner.SerialNo = loginMsg.SerialNo;
            miner.WalletAddress = loginMsg.WalletAddress;
            miner.ClientAddress = e.Client.Client.RemoteEndPoint.ToString();
            miner.Client = e.Client;
            miner.Stream = e.Stream;

            Random random = new Random();
            miner.CheckScoopNumber = random.Next(0, POC.MAX_SCOOP_NUMBER + 1);
            this.MinerList.Add(miner);

            //skip max nonce command
            //this.sendMaxNonceCommand(e, miner.CheckScoopNumber);

            miner.IsConnected = true;
            miner.ConnectedTime = Time.EpochTime;
            miner.LatestHeartbeatTime = Time.EpochTime;
            this.sendLoginResultCommand(e, true);
            LogHelper.Info(miner.ClientAddress + " login success");

            if(this.MiningStatus == MiningStatus.Mining)
            {
                StartMsg msg = new StartMsg();
                msg.BlockHeight = this.currentHeight;
                msg.ScoopNumber = this.currentScoopNumber;
                msg.StartTime = this.miningStartTime;
                msg.GenHash = GenHash(currentBlock.Header.PayloadHash, currentBlock.Header.Height);

                var startCmd = PoolCommand.CreateCommand(CommandNames.Start, msg);
                this.sendStartCommand(new TcpState { Client = miner.Client, Stream = miner.Stream, ClientAddress = miner.ClientAddress, Data = startCmd }, startCmd);
            }
        }

        private void receivedNonceDataCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var msg = new NonceDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.FirstOrDefault(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString());

            if (miner == null)
            {
                this.sendRejectCommand(e);
                return;
            }
            
            var data = POC.CalculateScoopData(miner.WalletAddress, msg.MaxNonce, miner.CheckScoopNumber);

            if (Base16.Encode(data) == Base16.Encode(msg.ScoopData))
            {
                miner.IsConnected = true;
                miner.ConnectedTime = Time.EpochTime;
                miner.LatestHeartbeatTime = Time.EpochTime;
                this.sendLoginResultCommand(e, true);
                LogHelper.Info(miner.ClientAddress + " login success");
            }
            else
            {
                this.sendLoginResultCommand(e, false);
                this.sendRejectCommand(e);
                LogHelper.Info(miner.ClientAddress + " login fail");
            }

        }

        private void receivedScoopDataCommand(TcpReceiveState e, PoolCommand cmd)
        {
            if (this.MiningStatus != MiningStatus.Mining)
                return;

            var msg = new ScoopDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.FirstOrDefault(m => m.ClientAddress == e.ClientAddress && m.IsConnected);

            if (miner == null)
            {
                LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce);
                LogHelper.Info("Miner logout");
                return;
            }

            //if (!ScoopDataRule.Current.AddToDic(msg.WalletAddress, msg.Nonce))
            //{
            //    LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce);
            //    LogHelper.Info("Nonce Height invalid,Nonce must Increment if not restart Mining");
            //    return;
            //}

            if (msg.BlockHeight != currentHeight)
            {
                LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce + ", height is " + msg.BlockHeight);
                LogHelper.Info("Block Height invalid");
                //this.sendRejectCommand(e);
                //this.MinerList.Remove(miner);
                return;
            }

            if (msg.ScoopNumber != this.currentScoopNumber)
            {
                LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce + ", ScoopNumber is " + this.currentScoopNumber + "/" + msg.ScoopNumber);
                LogHelper.Info("Scoop Number invalid");
                //this.MinerList.Remove(miner);
                //this.sendRejectCommand(e);
                return;
            }

            LogHelper.Info("Received scoop data from " + miner.ClientAddress + ", nonce is " + msg.Nonce + ", scoop number is " + msg.ScoopNumber + ", block height is " + msg.BlockHeight);

            this.lastScoopDataTime = Time.EpochTime;
            this.minerLastDataTime[msg.WalletAddress] = this.lastScoopDataTime;

            if (!this.minerEffort_OneStartTask.ContainsKey(miner.WalletAddress))
            {
                this.minerEffort_OneStartTask[miner.WalletAddress] = 1;
            }
            else
            {
                //if (minerEffort_OneStartTask[miner.WalletAddress] == MaxNonceCount)
                //{
                //    this.sendRejectCommand(e);
                //    return;
                //}
                this.minerEffort_OneStartTask[miner.WalletAddress]++;
            }

            this.totalEffort++;

            if (MiningPoolSetting.PoolType != "Server" || !POC.Verify(baseTarget, msg.Target))
                return;

            LogHelper.Info($"Find the correct Nonce {msg.Nonce}, from Miner {msg.WalletAddress}");
            var result = this.forgeBlock(msg.WalletAddress, msg.Nonce, msg.Target);
            LogHelper.Info($"forgeBlock result is {result}, Nonce ={msg.Nonce}, from Miner {msg.WalletAddress}");

            if (!result)
                return;

            if (result && currentBlock.Header.Height > 0)
            {
                string data = "";
                var redisKey = "MiningPool:MinerEffor";

                if (GlobalParameters.IsTestnet)
                {
                    redisKey = "MiningPool:Testnet:MinerEffor";
                }


                try
                {
                    data = redisDb.StringGet(redisKey);
                }
                catch (Exception ex)
                {
                    data = redisDb.StringGet(redisKey);
                }

                Dictionary<string, long> dictEffort = null;
                if (!string.IsNullOrWhiteSpace(data))
                {
                    dictEffort = JsonConvert.DeserializeObject<Dictionary<string, long>>(data);

                    foreach (var key in dictEffort.Keys)
                    {
                        totalEffort += dictEffort[key];
                    }
                }

                generatedBlockCount++;
                var totalReward = currentBlock.Transactions[0].Outputs[0].Amount;

                Blocks blocks = new Blocks();
                blocks.Generator = currentBlock.Header.GeneratorId;
                blocks.Hash = currentBlock.Header.GetHash();
                blocks.Height = currentBlock.Header.Height;
                blocks.Nonce = currentBlock.Header.Nonce;
                blocks.TotalHash = totalEffort;
                blocks.TotalReward = totalReward;

                AddEffort2MinerEffort();

                var hasRewardMiners = minerEffort.Where(x => x.Value > 0);
                var rewardAddresses = hasRewardMiners.Select(x => x.Key);
                List<RewardList> rewards = new List<RewardList>();
                foreach (var minerAddress in rewardAddresses)
                {
                    RewardList rewardList = new RewardList();
                    rewardList.BlockHash = blocks.Hash;
                    rewardList.GenerateTime = blocks.Timstamp;
                    rewardList.MinerAddress = minerAddress;
                    var hashCount = minerEffort[minerAddress];
                    rewardList.Hashes = hashCount;
                    rewardList.OriginalReward = (long)Math.Floor(totalReward * ((double)hashCount / (double)totalEffort));
                    rewardList.TransactionHash = "";
                    rewards.Add(rewardList);
                }

                if(dictEffort != null)
                {
                    hasRewardMiners = dictEffort.Where(x => x.Value > 0);
                    rewardAddresses = hasRewardMiners.Select(x => x.Key);
                    //rewards = new List<RewardList>();
                    foreach (var minerAddress in rewardAddresses)
                    {
                        RewardList rewardList = new RewardList();
                        rewardList.BlockHash = blocks.Hash;
                        rewardList.GenerateTime = blocks.Timstamp;
                        rewardList.MinerAddress = minerAddress;
                        var hashCount = dictEffort[minerAddress];
                        rewardList.Hashes = hashCount;
                        rewardList.OriginalReward = (long)Math.Floor(totalReward * ((double)hashCount / (double)totalEffort));
                        rewardList.TransactionHash = "";
                        rewards.Add(rewardList);
                    }
                }


                var blockComponent = new BlocksComponent();
                blockComponent.SaveBlockAndRewardLists(blocks, rewards);

                this.minerEffort.Clear();
                this.totalEffort = 0;
                this.MiningTimes = 0;

                MiningStatus = MiningStatus.Wait;
            }
        }

        private void AddEffort2MinerEffort()
        {
            foreach (var item in minerEffort_OneStartTask)
            {
                if (minerEffort.ContainsKey(item.Key))
                {
                    minerEffort[item.Key] += item.Value;
                }
                else
                {
                    minerEffort[item.Key] = item.Value;
                }
            }

            minerEffort_OneStartTask.Clear();
        }

        private void receivedHeartbeatCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var miner = this.MinerList.Where(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString() && m.IsConnected).FirstOrDefault();

            if (miner != null)
            {
                miner.LatestHeartbeatTime = Time.EpochTime;
            }
        }

        private void sendRegistResultCommand(TcpState e, bool result)
        {
            var msg = new RegistResultMsg();
            msg.Result = result;
            var cmd = PoolCommand.CreateCommand(CommandNames.RegistResult, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendLoginResultCommand(TcpState e, bool result)
        {
            var msg = new LoginResultMsg();
            msg.Result = result;
            var cmd = PoolCommand.CreateCommand(CommandNames.LoginResult, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendMaxNonceCommand(TcpState e, int scoopNumber)
        {
            var msg = new MaxNonceMsg();
            msg.RandomScoopNumber = scoopNumber;
            var cmd = PoolCommand.CreateCommand(CommandNames.MaxNonce, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendStartCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendStopCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendRewardCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendRejectCommand(TcpState e)
        {
            var rejectCmd = PoolCommand.CreateCommand(CommandNames.Reject, null);
            this.server.SendCommand(e, rejectCmd);
        }

        private byte[] GenHash(string payloadHash, long blockHeight)
        {
            var payloadBytes = Base16.Decode(payloadHash);
            var heightBytes = BitConverter.GetBytes(blockHeight);
            var hashSeed = new List<byte>();
            hashSeed.AddRange(payloadBytes);
            hashSeed.AddRange(heightBytes);
            return Sha3Helper.Hash(hashSeed.ToArray());
        }
    }

    public enum MiningStatus
    {
        Wait,
        Mining,
        Saving
    }
}