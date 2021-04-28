// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.Agent
{
    public class BlocksAPI
    {
        private static BlocksAPI _current;
        public static BlocksAPI Current
        {
            get
            {
                if (_current == null)
                    _current = new BlocksAPI();
                return _current;
            }
        }

        public Blocks SaveBlocks(Blocks entity)
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                var blocks= component.SaveBlock(entity);
                return blocks;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }
        }

        public async void GetVerifiedHashes()
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                await component.GetVerifiedHashes();
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        public void SaveBlockRates(long blocks, long difficulty)
        {
            try
            {
                BlockRatesComponent component = new BlockRatesComponent();
                component.SaveBlockRates(blocks, difficulty);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }
    }
}