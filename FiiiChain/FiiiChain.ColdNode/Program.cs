using FiiiChain.Business;
using FiiiChain.Framework;
using System;

namespace FiiiChain.ColdNode
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            bool testnet = false;

            if (args.Length == 1 && args[0].ToLower() == "-testnet")
            {
                testnet = true;
                LogHelper.Info("FiiiChain Testnet Engine is Started.");
            }
            else
            {
                LogHelper.Info("FiiiChain Engine is Started.");
            }

            try
            {
                GlobalParameters.IsTestnet = testnet;
                //开启BlockChain服务
                BlockchainJob.Initialize();
                BlockchainJob.Current.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
            
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                var ex = e.ExceptionObject as Exception;
                LogHelper.Error(ex.Message, ex);
            }
        }
    }
}
