// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiiiChain.PosPool.Apis
{
    internal class MinerApi
    {
        public static bool ValidateMiner(string address, string sn)
        {
            try
            {
                MinersComponent component = new MinersComponent();
                var miner = component.GetMinerByAddress(address);
                if (miner == null || miner.SN != sn || miner.Status != 0)
                {
                    bool isValidate = component.MinerLogin(address, sn);
                    return isValidate;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return false;
            }
        }

        public static List<Miners> GetAllMiners()
        {
            MinersComponent component = new MinersComponent();
            var miners = component.GetAllMiners().Where(x=>x.Status == 0).ToList();
            return miners;
        }

        public static Miners SaveMiners(string address, string account, string sn)
        {
            try
            {
                MinersComponent component = new MinersComponent();
                Miners entity = component.RegisterMiner(address, account, sn);
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return null;
            }
        }
    }
}
