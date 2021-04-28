using FiiiChain.Framework;
using FiiiChain.ShareModels.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool
{
    public class PoolCache
    {
        //internal static SafeCollection<Miner> WorkingMiners = new SafeCollection<Miner>();
        internal static SafeCollection<PoolTask> poolTasks = new SafeCollection<PoolTask>();

        internal static Dictionary<long, List<EffortInfo>> Efforts = new Dictionary<long, List<EffortInfo>>();

        internal static PoolTask CurrentTask = null;
    }
}
