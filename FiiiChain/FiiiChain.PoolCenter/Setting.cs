// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.PoolCenter
{
    public class Setting
    {
        private static string API_URL_MAIN = "http://localhost:5007/";
        private static string API_URL_TEST = "http://localhost:5006/";

        public static string API_URL
        {
            get
            {
                if (GlobalParameters.IsTestnet)
                {
                    return API_URL_TEST;
                }
                else
                {
                    return API_URL_MAIN;
                }
            }
        }

        /// <summary>
        /// 最大的缓存带挖矿数量
        /// </summary>
        public const int MaxMiningBlockCount = 100;

        internal const int SAVE_REWARDS_BEHIND_GENERATETIME_BLOCK = 1000 * 60 * 10;//10Min

        internal const int MAX_HEART_TIME = 1000 * 60 * 3; //3MIN

        private const string GROUP_NAME_MAIN = "poolcenter1";
        private const string GROUP_NAME_TEST = "poolcentertest1";

        internal static string CENTERKAFKAGROUPNAME
        {
            get
            {
                return GlobalParameters.IsTestnet ? GROUP_NAME_TEST : GROUP_NAME_MAIN;
            }
        }

        internal const int SS = 5 * 1000;
    }
}
