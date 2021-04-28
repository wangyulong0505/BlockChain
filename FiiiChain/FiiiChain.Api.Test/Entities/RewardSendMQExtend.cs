using FiiiChain.AliMQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Api.Test.Entities
{
    public class RewardSendMQExtend : RewardSendMQ
    {
        public string TransactionHash { get; set; }
    }
}
