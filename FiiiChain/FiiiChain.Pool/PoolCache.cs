// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.Pool.Models;
using FiiiChain.ShareModels.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Pool
{
    public class PoolCache
    {
        internal static SafeCollection<Miner> WorkingMiners = new SafeCollection<Miner>();
        internal static SafeCollection<PoolTask> poolTasks = new SafeCollection<PoolTask>();

        internal static Dictionary<long, List<EffortInfo>> Efforts = new Dictionary<long, List<EffortInfo>>();

        internal static PoolTask CurrentTask = null;
    }
}
