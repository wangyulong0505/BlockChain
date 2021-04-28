using FiiiChain.Pool.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiteDbTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DbHelper.Current = new DbHelper();
            var id = DbHelper.Current.Put("Test", new string[] { "1", "2", "3" });

            var value = DbHelper.Current.Get<string[]>("Test", id);

        }
    }
}
