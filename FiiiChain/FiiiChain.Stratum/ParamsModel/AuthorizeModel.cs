using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Stratum.ParamsModel
{
    public class AuthorizeModel
    {
        public string WalletAddress { get; set; }
        public int MinerType { get; set; }
        public string SerialNo { get; set; }
    }
}
