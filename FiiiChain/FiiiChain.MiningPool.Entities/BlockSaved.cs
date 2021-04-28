using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Entities
{
    public class BlockSaved
    {
        public Blocks block { get; set; }

        public List<RewardList> RewardLists { get; set; }
    }
}
