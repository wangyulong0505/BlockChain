// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.PoolCenter.Apis
{
    public class Validater
    {
        public static bool PoolAccount(string account)
        {
            return AccountIdHelper.AddressVerify(account);
        }
    }
}
