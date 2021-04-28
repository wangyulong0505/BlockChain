// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using ConvertUserToLevelDb.Db;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertUserToLevelDb
{
    public class DbDomains
    {
        public const string UserFileMainnet = "Temp/User";

        public static LevelDomain UserDomain;

        public static void Init()
        {
            UserDomain = new LevelDomain(UserFileMainnet);
        }
    }
}
