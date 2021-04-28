using FiiiChain.Data;
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Interfaces
{
    public interface IBlockchain
    {
        Block LastBlock { get; }

        Block GetLastBlock(long timestamp);

        long Height { get; }

        Block GetBlock(long blockId);

        Block GetBlockAtHeight(int height);

        bool HasBlock(long blockId);

        Dictionary<long, Block> AllBlocks { get; }

        Dictionary<long, Block> GetBlocks(int from, int to);

        IList<long> GetBlockIdsAfter(long blockId, int limit);

        Dictionary<long, Block> GetBlocksAfter(long blockId, int limit);

        long GetBlockIdAtHeight(int height);

        Transaction GetTransaction(long transactionId);

        Transaction GetTransactionByFullHash(string fullHash);

        bool HasTransaction(long transactionId);

        bool HasTransactionByFullHash(string fullHash);

        int TransactionCount { get; }

        IList<Transaction> AllTransactions { get; }

    }
}
