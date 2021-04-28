// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertUserToLevelDb.Db
{
    public class SettingDac : UserDbBase<SettingDac>
    {
        public Setting Setting = null;

        public SettingDac()
        {
            Setting = GetAppSetting();
        }

        #region Setting
        public Setting GetAppSetting()
        {
            Setting setting = new Setting();
            var feePerKB = UserDomain.Get(UserSetting.FeePerKB);
            var passCiphertext = UserDomain.Get(UserSetting.PassCiphertext);
            var encrypt = UserDomain.Get(UserSetting.Encrypt);

            setting.Confirmations = 1;
            long fee;
            if (feePerKB != null && long.TryParse(feePerKB, out fee))
            {
                setting.FeePerKB = fee;
            }
            else
            {
                setting.FeePerKB = 100000;
            }

            bool isEncrypt;
            if (encrypt != null && bool.TryParse(encrypt, out isEncrypt))
            {
                setting.Encrypt = isEncrypt;
            }
            else
            {
                setting.Encrypt = false;
            }

            setting.PassCiphertext = passCiphertext;
            return setting;
        }

        public void SetAppSetting(Setting setting)
        {
            Setting = setting;
            UserDomain.Put(UserSetting.Encrypt, setting.Encrypt.ToString());
            UserDomain.Put(UserSetting.FeePerKB, setting.FeePerKB.ToString());
            UserDomain.Put(UserSetting.PassCiphertext, setting.PassCiphertext);
        }
        #endregion
    }
}
