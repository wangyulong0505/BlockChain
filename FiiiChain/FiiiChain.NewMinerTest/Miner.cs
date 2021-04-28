using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using FiiiChain.Stratum;
using FiiiChain.Stratum.MinerSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.NewMinerTest
{
    class Miner : IDisposable
    {
        #region 

        public string PoolServerAddress { get; set; }
        public int PoolServerPort { get; set; }
        public string SerialNo { get; set; }
        public EnumMinerType MinerType { get; set; }
        public string WalletAddress { get; set; }
        public string PlotFilePath { get; set; }
        public long Capacity { get; set; }
        public long TotalCountofNonce { get; set; }

        #endregion

        #region

        private SocketClient socketClient;
        private int nonceSize = 64 * 4096;
        private int noncePerFile = 100;
        private int scoopDataLen = 64;
        private bool isInMinging = false;
        private bool isRegist = false;
        private int difficulty = 0;
        private BigInteger target_1 = BigInteger.Parse("00000000FFFFFF00000000000000000000000000000000000000000000000000", NumberStyles.AllowHexSpecifier);

        #endregion

        /// <summary>
        /// 矿工初始化
        /// </summary>
        /// <param name="isMinerRegist">矿工是否注册</param>
        public void Init(bool isMinerRegist = false)
        {
            IPAddress ip;
            //解析IP地址
            if (!IPAddress.TryParse(this.PoolServerAddress, out ip))
            {
                try
                {
                    var ips = Dns.GetHostAddresses(this.PoolServerAddress);

                    if (ips.Length > 0)
                    {
                        ip = ips[0];
                    }
                    else
                    {
                        throw new CommonException(ErrorCode.Engine.P2P.Connection.HOST_NAME_CAN_NOT_RESOLVED_TO_IP_ADDRESS);
                    }
                }
                catch
                {
                    throw new CommonException(ErrorCode.Engine.P2P.Connection.HOST_NAME_CAN_NOT_RESOLVED_TO_IP_ADDRESS);
                }
            }
            //初始化SocketTcpClient
            socketClient = new SocketClient(new IPEndPoint(ip, this.PoolServerPort));
            socketClient.ConnectStatusChangedAction = ConnectStatusChanged;
            socketClient.ReceivedCommandAction = ReceivedCommand;
            socketClient.ProcessErrorAction = SocketErrorReceived;
            isRegist = isMinerRegist;
        }

        /// <summary>
        /// 开启TCP连接
        /// </summary>
        public void Start()
        {
            socketClient.Connect();
        }

        /// <summary>
        /// 断开TCP连接
        /// </summary>
        public void Stop()
        {
            socketClient.Close();
        }

        #region File Init

        /// <summary>
        /// 初始化数据文件
        /// </summary>
        public void InitPlotFiles()
        {
            TotalCountofNonce = Capacity / nonceSize;

            var origRow = Console.CursorTop;
            var origCol = Console.CursorLeft;

            var startIndex = GetScoopStartIndex();

            if (startIndex < 0)
            {
                Console.SetCursorPosition(origCol, origRow);
                Console.Write("Nonce Progress: " + TotalCountofNonce.ToString() + " / " + TotalCountofNonce.ToString());
            }
            var groups = (int)Math.Ceiling((double)(TotalCountofNonce) / (double)noncePerFile);
            long finishedNonce = startIndex - 1;
            long maxFiles = groups * 4096L;

            var startG = startIndex / noncePerFile;

            for (int g = startG; g < groups; g++)
            {
                var min = g * (long)noncePerFile;
                var max = (g + 1) * (long)noncePerFile - 1L;

                if (max >= TotalCountofNonce)
                {
                    max = TotalCountofNonce - 1;
                }

                var nonceData = new ConcurrentBag<NonceData>();

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Nonce calculation started. Min: " + min.ToString() + " Max: " + max.ToString());
                Parallel.For(min, max, i => {
                    var data = POC.GenerateNonceData(WalletAddress, i);
                    nonceData.Add(data);
                    finishedNonce++;
                });
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Nonce calculation finished.");
                Console.WriteLine("Nonce Progress: " + (finishedNonce + 1).ToString().PadLeft((TotalCountofNonce - 1).ToString().Length, '0') + " / " + TotalCountofNonce.ToString());

                var copyData = nonceData.OrderBy(n => n.Nonce).ToArray();
                Task.Factory.StartNew(() => this.SaveNonce(PlotFilePath, WalletAddress, copyData, max, (TotalCountofNonce - 1).ToString().Length));
            }

            Console.WriteLine();
            Console.WriteLine("Init finished " + DateTime.Now.ToString());
            System.Threading.Thread.Sleep(5000);
        }

        /// <summary>
        /// 获取初始的Scoop索引，没有生成完接着生成
        /// </summary>
        /// <returns></returns>
        public int GetScoopStartIndex()
        {
            int startScoop = 0;
            //数据文件夹是否存在
            if (!Directory.Exists(PlotFilePath))
            {
                return startScoop;
            }
            //文件是以文件夹的形式存放的，文件夹名称示例：Scoop_415
            string[] scoopDirs = Directory.GetDirectories(PlotFilePath);
            if (!scoopDirs.Any())
            {
                return startScoop;
            }
            var files = Directory.GetFiles(PlotFilePath + "/Scoop_4095");
            string fileName = null;
            if (files != null)
            {
                Array.Sort(files);
                fileName = files.LastOrDefault();
            }
            //最后一个文件的文件名
            if (string.IsNullOrEmpty(fileName))
            {
                return startScoop;
            }
            //文件示例：fiiit4MBt7EpAFx8VcFdVSbAbDp1s4g4HfKA61_Nonce_0600_0699
            string[] fs = Path.GetFileNameWithoutExtension(fileName).Split('_');
            var minNonce = int.Parse(fs[2]);
            var maxNonce = int.Parse(fs[3]);

            for (int i = 4095; i <= 0; i--)
            {
                var dirPath = Path.Combine(PlotFilePath, "Scoop_" + i);
                if (!Directory.Exists(PlotFilePath) || !Directory.Exists(dirPath))
                {
                    return minNonce;
                }
                var noncefiles = Directory.GetFiles(WalletAddress + "_Nonce" + "_*");
                if (noncefiles == null || !noncefiles.Any(x => x.Equals(fileName)))
                {
                    return minNonce;
                }
            }
            //数据文件初始化完成了
            if (maxNonce == TotalCountofNonce)
            {
                return -1;
            }
            //如果文件夹中的文件全部生成
            if ((maxNonce + 1) % noncePerFile > 0)
            {
                startScoop = minNonce;
            }
            else
            {
                startScoop = maxNonce + 1;
            }
            return startScoop;
        }

        /// <summary>
        /// Nonce文件写进磁盘
        /// </summary>
        /// <param name="location"></param>
        /// <param name="address"></param>
        /// <param name="nonceData"></param>
        /// <param name="maxNonce"></param>
        /// <param name="endNonceLen"></param>
        private void SaveNonce(string location, string address, NonceData[] nonceData, long maxNonce, int endNonceLen)
        {
            System.Threading.Thread.CurrentThread.IsBackground = false;
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            var minNonceText = ((maxNonce / noncePerFile) * noncePerFile).ToString().PadLeft(endNonceLen, '0');
            var maxNonceText = maxNonce.ToString().PadLeft(endNonceLen, '0');
            var fileName = string.Format("{0}_Nonce_{1}_{2}", address, minNonceText.PadLeft(4, '0'), maxNonceText.PadLeft(4, '0'));

            Parallel.For(0, 4096, i =>
            {
                var dirName = Path.Combine(PlotFilePath, "Scoop_" + i);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                var path = Path.Combine(dirName, fileName);
                using (var file = new FileStream(path, FileMode.Create))
                {
                    var scoopDatas = nonceData.SelectMany(x => x.DataList[i].FullData).ToArray();

                    file.Write(scoopDatas, 0, scoopDatas.Length);

                    file.Flush();
                }
            });
        }

        /// <summary>
        /// 保存生成文件的配置
        /// </summary>
        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(this);
            var path = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Setting.json");

            using (var fileStream = File.Open(path, FileMode.Create))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                fileStream.Write(bytes, 0, bytes.Length);

                fileStream.Flush();
            }
        }

        /// <summary>
        /// 从配置文件中加载
        /// </summary>
        /// <returns></returns>
        public static Miner LoadFromSetting()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(Miner).Assembly.Location), "Setting.json");
            var json = File.ReadAllText(path);

            var miner = JsonConvert.DeserializeObject<Miner>(json);
            return miner;
        }

        #endregion

        /// <summary>
        /// 上传Nonce数据
        /// </summary>
        /// <param name="startMsg"></param>
        private async void StartSendScoopData(StartMsg startMsg)
        {
            string scoop = startMsg.ScoopNumber.ToString();

            byte[] currentBytes = new byte[scoopDataLen];
            string scoopDir = Path.Combine(PlotFilePath, "Scoop_" + scoop);
            string[] files = Directory.GetFiles(scoopDir, WalletAddress + "_Nonce" + "_*");
            Array.Sort(files);
            int currentNonce = -1;
            //var newTarget = target_1 * (long)1E15 / new BigInteger(Math.Round(difficulty * 1.0, 15) * 1.0E15);

            foreach (var file in files)
            {
                //二级制文件格式：fiiimRTsPRrWQCaCFRFb6XpKV1BtoNuyny4kDL_Nonce_1000_1099
                var fileData = Path.GetFileNameWithoutExtension(file).Split('_');
                var minNonce = int.Parse(fileData[2]);
                var maxNonce = int.Parse(fileData[3]);

                var bytes = File.ReadAllBytes(file);
                var index = 0;
                var bytesLen = bytes.Length;
                currentNonce = minNonce;

                while (index < bytesLen && isInMinging)
                {
                    await Task.Run(() =>
                    {
                        currentBytes = new byte[scoopDataLen];
                        Array.Copy(bytes, index, currentBytes, 0, currentBytes.Length);

                        List<byte> targetByteLists = new List<byte>();
                        targetByteLists.AddRange(currentBytes);
                        targetByteLists.AddRange(startMsg.GenHash);
                        byte[] baseTarget = Sha3Helper.Hash(targetByteLists.ToArray());
                        //判断baseTarget和难度值大小决定是否上传数据

                        this.SendScoopDataCommand(startMsg.BlockHeight, currentNonce, startMsg.ScoopNumber, baseTarget);

                        Console.WriteLine("Progress: " + currentNonce.ToString().PadLeft((TotalCountofNonce - 1).ToString().Length, '0') + " / " + (TotalCountofNonce - 1).ToString());
                    });

                    Task.Delay(20).Wait();
                    index += scoopDataLen;
                    currentNonce += 1;
                }
                if (!isInMinging)
                {
                    LogHelper.Info("Mining is stopped");
                    return;
                }
            }

            Console.WriteLine();
            Console.WriteLine("All Nonces have been sent");
        }

        /// <summary>
        /// 连接状态改变
        /// </summary>
        /// <param name="connected"></param>
        private void ConnectStatusChanged(bool connected)
        {
            if (connected && !isRegist)
            {
                LogHelper.Info("Connect successed, start to login");
                //调用stratum的Login接口
                SendModel model = new SendModel
                {
                    Id = (int)MethodId.Authorize,
                    Method = MethodName.Authorize_Name,
                    Params = new object[] { WalletAddress, SerialNo, MinerType }
                };
                SendAuthorizeCommand(model);
            }
            else
            {
                LogHelper.Info("Connect to MiningPool Fail , Wait ReConnect");
                Task.Delay(3000).Wait();
                Program.StartMining(this);
            }
        }

        /// <summary>
        /// 收到SocketError信息
        /// </summary>
        /// <param name="errorCode"></param>
        private void SocketErrorReceived(int errorCode)
        {
            this.isInMinging = false;
            LogHelper.Error("Socket error received " + ((SocketError)errorCode).ToString());
        }

        #region Command命令

        /// <summary>
        /// 收到命令
        /// </summary>
        /// <param name="cmd"></param>
        private void ReceivedCommand(byte[] data)
        {
            string info = Encoding.UTF8.GetString(data);
            DataModel input = JsonConvert.DeserializeObject<DataModel>(info);
            switch (input.Id)
            {
                case (int)MethodId.Authorize:
                    ReceiveAuthorizeCommand(data);
                    break;
                case (int)MethodId.Subscribe:
                    ReceiveSubscribeCommand(data);
                    break;
                case (int)MethodId.Notify:
                    ReceiveNotifyCommand(data);
                    break;
                case (int)MethodId.Submit:
                    ReceiveSubmitCommand(data);
                    break;
                case (int)MethodId.Diffculty:
                    ReceiveDiffcultyCommand(data);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 发送账号登录命令
        /// </summary>
        /// <param name="model"></param>
        private void SendAuthorizeCommand(SendModel model)
        {
            try
            {
                socketClient.Send(StratumHelper.ObjectToByte(model));
            }
            catch(Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 接收账号登录命令
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveAuthorizeCommand(byte[] data)
        {
            try
            {
                //这里会收到Server的Replay消息
                ReplyModel replyModel = StratumHelper.ByteToObject<ReplyModel>(data);
                if(Convert.ToBoolean(replyModel.Result[0]))
                {
                    SendModel model = new SendModel
                    {
                        Id = (int)MethodId.Subscribe,
                        Method = MethodName.Subscribe_Name,
                        Params = new object[] { }
                    };
                    //发送矿机登记命令
                    SendSubscribeCommand(model);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                //授权失败等待三秒重新授权
                Task.Delay(3000).Wait();
                ConnectStatusChanged(true);
            }
        }

        /// <summary>
        /// 发送矿机登记命令
        /// </summary>
        private void SendSubscribeCommand(SendModel model)
        {
            try
            {
                socketClient.Send(StratumHelper.ObjectToByte(model));
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 接收矿机登记命令
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveSubscribeCommand(byte[] data)
        {
            try
            {
                //这里会收到Server的Replay消息
                ReplyModel model = StratumHelper.ByteToObject<ReplyModel>(data);
                difficulty = Convert.ToInt32(model.Result[0]);
                //业务处理
                if(difficulty <= 0)
                {
                    throw new Exception(model.Error.ToString());
                }
                isRegist = true;
                LogHelper.Info("Subscribe successed, waiting for mining task.");
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 接收矿池分配任务命令
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveNotifyCommand(byte[] data)
        {
            try
            {
                //这里会收到Server的Replay消息
                ReplyModel model = StratumHelper.ByteToObject<ReplyModel>(data);
                if (Convert.ToBoolean(model.Result[0]))
                {
                    //TODO业务处理,开始上传数据

                    var msg = new StartMsg();
                    int index = 0;
                    msg.Deserialize(data, ref index);

                    LogHelper.Info("Received new block " + msg.BlockHeight + " mining task");
                    this.isInMinging = true;
                    this.StartSendScoopData(msg);
                }
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 发送数据命令
        /// </summary>
        /// <param name="height"></param>
        /// <param name="nonce"></param>
        /// <param name="scoopNumber"></param>
        /// <param name="target"></param>
        private void SendScoopDataCommand(long height, long nonce, int scoopNumber, byte[] target)
        {
            SendModel model = new SendModel
            {
                Id = (int)MethodId.Submit,
                Method = MethodName.Submit_Name,
                Params = new object[] { Guid.NewGuid(), WalletAddress, height, nonce, scoopNumber, target }
            };
            SendSubmitCommand(model);
        }

        /// <summary>
        /// 发送结果提交命令
        /// </summary>
        /// <param name="model"></param>
        private void SendSubmitCommand(SendModel model)
        {
            try
            {
                socketClient.Send(StratumHelper.ObjectToByte(model));
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 接收结果提交命令
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveSubmitCommand(byte[] data)
        {
            try
            {
                //这里会收到Server的Replay消息
                ReplyModel model = StratumHelper.ByteToObject<ReplyModel>(data);
                if(Convert.ToBoolean(model.Result[0]))
                {
                    //关闭连接
                    //Stop();
                    isInMinging = false;
                }
                else
                {
                    SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                }
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 接收难度调整命令
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveDiffcultyCommand(byte[] data)
        {
            try
            {
                //这里会收到Server的Replay消息
                ReplyModel model = StratumHelper.ByteToObject<ReplyModel>(data);
                int difficulty = Convert.ToInt32(model.Result[0]);
                //TODO业务处理，根据传递的难度值计算上传文件的难度，筛选数据是否上传
                //调用
            }
            catch (Exception ex)
            {
                SocketErrorReceived(ResultErrorCode.UNKNOWN_ERROR);
                LogHelper.Error(ex.ToString());
            }
        }

        #endregion

        public void Dispose()
        {
            socketClient = null;
        }
    }
}
