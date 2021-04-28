using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FiiiChain.AliMQ.Test
{
    [TestClass]
    public class MessageTest
    {
        [TestMethod]
        public void ProducerMessage()
        {
            ProducerMessage<RewardSendMQ>.ProducerMessageAction("test", new RewardSendMQ { Account = "account1", Address = "address1", Reward = 100, SN = "N30001F030" });
        }

        [TestMethod]
        public void ConsumerMessage()
        {
            List<RewardSendMQ> result = ConsumerMessage<RewardSendMQ>.ConsumerMessageFunc("test");
            Assert.IsNotNull(result);
        }
    }
}
