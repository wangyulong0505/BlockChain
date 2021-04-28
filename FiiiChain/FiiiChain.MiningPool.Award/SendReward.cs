using FiiiChain.DTO.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Award
{
    public class SendReward : SendRawTransactionOutputsIM
    {
        public long OriginalReward { get; set; }
    }
}
