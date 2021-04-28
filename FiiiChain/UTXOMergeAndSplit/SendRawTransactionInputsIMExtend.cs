using FiiiChain.DTO.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace UTXOMergeAndSplit
{
    public class SendRawTransactionInputsIMExtend : SendRawTransactionInputsIM
    {
        public long Amount { get; set; }
    }
}
