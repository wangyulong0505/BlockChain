// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Pool.Redis
{
    internal class Setting
    {
        public static string[] RedisConnections
        {
            get
            {
                return GlobalParameters.IsTestnet ? RedisConnections_Test : RedisConnections_Main;
            }
        }


        ////127.0.0.1:6371,password=123,defaultDatabase=11,poolsize=10,ssl=false,writeBuffer=10240,prefix=key前辍

        private static string[] RedisConnections_Test = new string[]
        {
            $"127.0.0.1:6379,{""},defaultDatabase=6,abortConnect=False"
        };

        private static string[] RedisConnections_Main = new string[]
        {
            "r-3ns10cea37457624.redis.rds.aliyuncs.com:6379,abo,abortConnect=False"
        };
    }
}