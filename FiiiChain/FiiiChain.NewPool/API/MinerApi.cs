using FiiiChain.Framework;
using FiiiChain.MiningPool.Business;
using System;

namespace FiiiChain.NewPool.API
{
    public class MinerApi
    {
        /// <summary>
        /// 矿工身份验证
        /// </summary>
        /// <param name="address"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static bool ValidateMiner(string address, string sn)
        {
            return true;
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
    }
}
