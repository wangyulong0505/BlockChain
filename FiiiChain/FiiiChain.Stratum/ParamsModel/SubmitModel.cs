using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSocket.Stratum.ParamsModel
{
    public class SubmitModel
    {
        public string BF { get; set; }
        public int BlockHeight { get; set; }
        public string WalletAddress { get; set; }
        public int Nonce { get; set; }
        public int ScoopNumber { get; set; }
        public byte[] Target { get; set; }
       
    }
}
