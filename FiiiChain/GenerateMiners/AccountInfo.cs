using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateMiners
{
    public class AccountInfo
    {
        public string Address { get; set; }

        public string PublicKey { get; set; }

        public long Balance { get; set; }

        public bool IsDefault { get; set; }

        public bool WatchOnly { get; set; }

        public string Tag { get; set; }
    }
}
