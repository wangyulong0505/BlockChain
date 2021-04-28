// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Rules
{
    public class ScoopDataRule
    {
        private static ScoopDataRule _rule;
        public static ScoopDataRule Current
        {
            get
            {
                if (_rule == null)
                    _rule = new ScoopDataRule();
                return _rule;
            }
        }

        Dictionary<string, long> ReceiveScoopDataDic = new Dictionary<string, long>();

        public void Clear()
        {
            ReceiveScoopDataDic.Clear();
        }

        public bool AddToDic(string address, long nonce)
        {
            if (ReceiveScoopDataDic.ContainsKey(address))
            {
                var lastNonce = ReceiveScoopDataDic[address];
                if (nonce - lastNonce != 1)
                    return false;
                else
                    ReceiveScoopDataDic[address] = nonce;
            }
            else
            {
                if (nonce != 0)
                    return false;
                ReceiveScoopDataDic.Add(address, nonce);
            }
            return true;
        }
    }
}
