using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateMiners
{
    public class GetTxSettings
    {
        public long Confirmations { get; set; }
        public long FeePerKB { get; set; }
        public bool Encrypt { get; set; }
    }
}
