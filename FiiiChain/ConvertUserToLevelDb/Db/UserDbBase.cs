// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertUserToLevelDb.Db
{
    public abstract class DacBase<T>
    {
        public static T Default = System.Activator.CreateInstance<T>();

        public string GetKey(string catelog, string key)
        {
            return catelog + "_" + key;
        }

        public string GetKey(string catelog, long key)
        {
            return GetKey(catelog, key.ToString());
        }

        public string GetKey(string catelog, params string[] ps)
        {
            return catelog + "_" + string.Join("_", ps);
        }
    }

    public abstract class UserDbBase<T> : DacBase<T>
    {
        protected LevelDomain UserDomain
        {
            get
            {
                return DbDomains.UserDomain;
            }
        }
        protected UserDbBase()
        {
        }
    }
}
