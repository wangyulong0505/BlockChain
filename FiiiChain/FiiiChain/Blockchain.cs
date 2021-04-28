using FiiiChain.Data;
using FiiiChain.Entities;
using FiiiChain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiiiChain
{
    public sealed class Blockchain : IBlockchain
    {
        private BlockDac blockDac;
        private TransactionDac txDac;

        public Blockchain()
        {
            blockDac = new BlockDac();
            txDac = new TransactionDac();
        }

        public Block LastBlock => blockDac.SelectLast();                    

        public long Height => LastBlock.Height;

        public Dictionary<long, Block> AllBlocks => blockDac.Select();

        public int TransactionCount => txDac.Count();

        public IList<Transaction> AllTransactions => txDac.Select();

        public Block GetBlock(long blockId)
        {
            Block block = blockDac.SelectById(blockId);
            block.Transactions = txDac.SelectByBlockHash(block.Hash);
            return block;
        }

        public Block GetBlockAtHeight(int height)
        {
            Block block = blockDac.SelectByHeight(height);
            block.Transactions = txDac.SelectByBlockHash(block.Hash);
            return block;
        }

        public long GetBlockIdAtHeight(int height)
        {
            return blockDac.SelectIdByHeight(height);
        }

        public IList<long> GetBlockIdsAfter(long blockId, int limit)
        {
            return blockDac.SelectIdByLimit(blockId, limit);
        }

        public Dictionary<long, Block> GetBlocks(int from, int to)
        {
            return blockDac.SelectByHeightRange(from, to);
        }

        public Dictionary<long, Block> GetBlocksAfter(long blockId, int limit)
        {
            return blockDac.SelectByLimit(blockId, limit);
        }

        public Block GetLastBlock(long timestamp)
        {
            Block block = LastBlock;
            if (timestamp <= block.Timestamp)
            {
                return block;
            }

            return blockDac.SelectLast();
        }

        public Transaction GetTransaction(long transactionId)
        {
            return txDac.SelectById(transactionId);
        }

        public Transaction GetTransactionByFullHash(string fullHash)
        {
            return txDac.SelectByHash(fullHash);
        }

        public bool HasBlock(long blockId)
        {
            return LastBlock.Id == blockId || blockDac.HasBlock(blockId);
        }

        public bool HasTransaction(long transactionId)
        {
            return txDac.HasTransaction(transactionId);
        }

        public bool HasTransactionByFullHash(string fullHash)
        {
            return txDac.HasTransactionByHash(fullHash);
        }
    }
}
