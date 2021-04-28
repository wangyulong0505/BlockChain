using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.API.Config
{
    public class ServerSetting
    {
        public long ServerRefreshTime { get; set; }

        public long MinerAmount { get; set; }

        public List<ServerInfo> ServerInfoList { get; set; }

        public bool IsTestNet { get; set; }
    }

    public class ServerInfo
    {
        public string Name { get; set; }

        public string IPAddress { get; set; }

        public int port { get; set; }

        public long MinerCount { get; set; }
    }
}
