// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System;

namespace FiiiChain.MiningPool.Agent
{
    public class MinersAPI
    {
        private static MinersAPI _current;
        public static MinersAPI Current
        {
            get
            {
                if (_current == null)
                    _current = new MinersAPI();
                return _current;
            }
        }

        public bool POSValidate(string address, string sn)
        {
            try
            {
                MinersComponent component = new MinersComponent();
                bool isValidate = component.MinerLogin(address, sn);
                return isValidate;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return false;
            }
        }

        public Miners SaveMiners(string address, string account, string sn)
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