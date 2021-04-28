using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.NewPool.Models
{
    internal class MinerEffort
    {
        internal string Account;
        internal int Effort;

        internal double Diffculty; //当前难度
        internal int CurrentNonce; //当前已上传Nonce，防重复传
    }
}
