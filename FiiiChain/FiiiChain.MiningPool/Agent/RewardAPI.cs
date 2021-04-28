// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiChain.Framework;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Agent
{
    public class RewardAPI
    {
        private static RewardAPI _current;
        public static RewardAPI Current
        {
            get
            {
                if (_current == null)
                    _current = new RewardAPI();
                return _current;
            }
        }

        public long GetUnPaidReward(string address)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                return component.GetUnPaidReward(address);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }
        }

        public long GetPaidReward(string address)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                return component.GetUnPaidReward(address);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void SaveReward(RewardList entity)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                component.InsertRewardList(entity);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }
        }

        public long GetActualReward(string address, string blockHash)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                var result = component.GetActualReward(address, blockHash);
                return result;
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
