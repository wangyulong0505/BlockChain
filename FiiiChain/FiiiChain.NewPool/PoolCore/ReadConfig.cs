using FiiiChain.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

namespace FiiiChain.NewPool
{
    public class SocketEntity
    {
        public string IP { get; set; }
        public int MainPort { get; set; }
        public int TestPort { get; set; }
        public int MaxConnectNum { get; set; }
        public int BufferSize { get; set; }
        public int ClientTimeOut { get; set; }

        public int GetPort()
        {
            if (Framework.GlobalParameters.IsTestnet)
            {
                return TestPort;
            }
            else
            {
                return MainPort;
            }
        }
    }

    public class PoolEntity
    {
        public string PoolId { get; set; }
        public int PoolHeartTime { get; set; }
        public int PoolSaveWorkTime { get; set; }
        public int MaxNonceCount { get; set; }
        public double DefaultDiffculty { get; set; }
    }

    class ReadConfig
    {
        private static IConfiguration Configuration { get; set; }
        public static SocketEntity SocketConfig;
        public static PoolEntity PoolConfig;

        static ReadConfig()
        {
            //ReloadOnChange = true 当appsettings.json被修改时重新加载            
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "PoolSettings.json", ReloadOnChange = true })
            .Build();

            SocketConfig = GetSocketConfig();
            PoolConfig = GetPoolConfig();
        }

        private static SocketEntity GetSocketConfig()
        {
            SocketEntity entity = null;

            try
            {
                entity = new SocketEntity();
                entity.IP = Configuration["Socket:IP"].ToString();
                entity.MainPort = Convert.ToInt32(Configuration["Socket:MainPort"]);
                entity.TestPort = Convert.ToInt32(Configuration["Socket:TestPort"]);
                entity.MaxConnectNum = Convert.ToInt32(Configuration["Socket:MaxConnectNum"]);
                entity.BufferSize = Convert.ToInt32(Configuration["Socket:BufferSize"]);
                entity.ClientTimeOut = Convert.ToInt32(Configuration["Socket:ClientTimeOut"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetSocketConfig is error:{ex.Message}");
            }
            return entity;

        }

        private static PoolEntity GetPoolConfig()
        {
            PoolEntity entity = null;
            try
            {
                entity = new PoolEntity();
                entity.PoolId = Configuration["Pool:PoolId"];
                entity.PoolHeartTime = Convert.ToInt32(Configuration["Pool:PoolHeartTime"]);
                entity.PoolSaveWorkTime = Convert.ToInt32(Configuration["Pool:PoolSaveWorkTime"]);
                entity.MaxNonceCount = Convert.ToInt32(Configuration["Pool:MaxNonceCount"]);
                entity.DefaultDiffculty = Convert.ToDouble(Configuration["Pool:DefaultDiffculty"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPoolConfig is error:{ex.Message}");
            }
            return entity;
        }
    }
}
