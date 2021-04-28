﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.DTO
{
    public class ListSinceBlockOM
    {
        public Payment[] transactions { get; set; }
        public string lastBlock { get; set; }

        public class Payment
        {
            public bool involvesWatchonly { get; set; }
            public string account { get; set; }
            public string address { get; set; }
            public string category { get; set; }
            public long amount { get; set; }
            public int vout { get; set; }
            public long fee { get; set; }
            public long confirmations { get; set; }
            public bool generated { get; set; }
            public string blockHash { get; set; }
            public int blockIndex { get; set; }
            public long blockTime { get; set; }
            public string txId { get; set; }
            public string[] walletconflicts { get; set; }
            public long time { get; set; }
            public long timereceived { get; set; }
            public string comment { get; set; }
            public string to { get; set; }

        }
    }
}
