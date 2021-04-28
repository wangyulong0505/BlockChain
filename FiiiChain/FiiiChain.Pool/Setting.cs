// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;

namespace FiiiChain.Pool
{
    public static class Setting
    {
        public static string PoolAddress;

        private const int POOL_MAIN_PORT = 5009;
        private const int POOL_TEST_PORT = 5008;

        public static int PoolPort
        {
            get
            {
                if (Framework.GlobalParameters.IsTestnet)
                {
                    return POOL_TEST_PORT;
                }
                else
                {
                    return POOL_MAIN_PORT;
                }
            }
        }
        
        public const int MaxNonceCount = 262144;

        public const int BufferSize = Int16.MaxValue;

        public static string PoolId = $"PoolId:{Guid.NewGuid().ToString()}";

        public const int HEART_TIME = 1000 * 10;

        public static int Max_TCP_Count = 800;
    }
}
