using FiiiChain.Data;
using System;
using System.Linq;

namespace ConvertUserToLevelDb
{
    class Program
    {
        static void Main(string[] args)
        {
            DbDomains.Init();

            FiiiChain.Framework.GlobalParameters.IsTestnet = false;
            Console.WriteLine(FiiiChain.Data.AccountDac.Default.CacheConnectionString);
            
            var accountBook = AccountDac.Default.SelectAll();
            Db.AccountDac.Default.Insert(accountBook);

            var addressBook = AddressBookDac.Default.SelectAll();
            Db.AddressBookDac.Default.InsertOrUpdate(addressBook);

            var setting = SettingDac.Default.GetSetting();
            Db.SettingDac.Default.SetAppSetting(setting);

            var newSetting = Db.SettingDac.Default.GetAppSetting();
            Console.WriteLine($"Accounts Count{Db.AccountDac.Default.SelectAll().Count()}/{accountBook.Count}");
            Console.WriteLine($"AddressBook Count{Db.AddressBookDac.Default.SelectAll().Count()}/{addressBook.Count}");
            Console.WriteLine($"Setting.Encrypt {newSetting.Encrypt}/{setting.Encrypt}");
            Console.WriteLine($"Setting.FeePerKB {newSetting.FeePerKB}/{setting.FeePerKB}");
            Console.WriteLine($"Setting.PassCiphertext {newSetting.PassCiphertext}/{setting.PassCiphertext}");

            Console.ReadKey();
        }
    }
}
