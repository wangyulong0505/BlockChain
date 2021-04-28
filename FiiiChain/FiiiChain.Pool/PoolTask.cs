// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.Messages;
using FiiiChain.Pool.Models;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Pool
{
    internal class PoolTask
    {
        internal PoolTask()
        {
            MinerEfforts = new SafeCollection<MinerEffort>();
            SavingMinerEfforts = new SafeCollection<MinerEffort>();
        }

        internal string Id;

        internal BlockMsg GeneratingBlock;

        internal StartMsg CurrentStartMsg;

        internal long BaseTarget;

        internal MiningState State;

        internal long CurrentBlockHeight;

        internal int CurrentScoopNumber;

        internal long LastReceiveTime;

        internal long StartTime;

        internal SafeCollection<MinerEffort> MinerEfforts;

        internal SafeCollection<MinerEffort> SavingMinerEfforts;
    }

    internal enum MiningState
    {
        Wait,
        Mining
    }

}
