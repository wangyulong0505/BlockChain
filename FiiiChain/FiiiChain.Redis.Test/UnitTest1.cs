using FiiiChain.Pool.Redis;
using System;
using Xunit;

namespace FiiiChain.Redis.Test
{
    public class Tests
    {
        [Fact]
        public void TestRedisConnect()
        {
            RedisManager.Current.SaveDataToRedis("1", "1");
            var v = RedisManager.Current.GetDataInRedis<string>("1");
        }
    }
}
