using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FiiiChain.KafkaMQ.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            MqManager.Current.Regist("SendMsg9", Get);
            MqManager.Current.Listen();
            Task.Delay(3000).Wait();

            MqManager.Current.Send("SendMsg9", "1|2|3");
            Task.Delay(5000).Wait();

            
        }

        public void Get(object param)
        {
            var s = param.ToString();
        }
    }
}
