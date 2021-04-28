// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertUserToLevelDb.Db
{
    public class WalletBackupDac:UserDbBase<WalletBackupDac>
    {
        public virtual int Restore(WalletBackup entity)
        {
            try
            {
                AccountDac.Default.Insert(entity.AccountList);
                AddressBookDac.Default.InsertOrUpdate(entity.AddressBookItemList);
                SettingDac.Default.SetAppSetting(entity.SettingList.FirstOrDefault());
                return 0;
            }
            catch
            {
                return -1;
            }
        }
    }
}