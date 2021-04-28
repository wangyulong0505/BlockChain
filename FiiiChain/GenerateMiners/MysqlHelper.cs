using FiiiChain.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateMiners
{
    public class MysqlHelper
    {
        private const string CONNECTIONSTRING = "server=rm-3nsnh13p454nv0r68.mysql.rds.aliyuncs.com;port=3306;user=fiiipool_dbo;password=8&KujS!sCw3Ex26f;database=fiiipool_test;SslMode=none;ConnectionTimeout=1200";

        public void InsertMiners(string address, string sn, string account)
        {
            LogHelper.Info($"Insert into miners table address = {address}, sn = {sn}, account = {account}");
            string SQL_STATEMENT = "INSERT INTO Miners (Address, Account, `Type`, SN, `Status`, Timstamp, LastLoginTime, PaidReward, UnpaidReward) VALUES(@Address, @Account, 0, @SN, 0, @Timstamp, @LastLoginTime, 0, 0); ";
            using (MySqlConnection conn = new MySqlConnection(CONNECTIONSTRING))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL_STATEMENT, conn))
                {
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Account", account);
                    cmd.Parameters.AddWithValue("@SN", sn);
                    cmd.Parameters.AddWithValue("@Timstamp", Time.EpochTime);
                    cmd.Parameters.AddWithValue("@LastLoginTime", Time.EpochTime);

                    cmd.Connection.Open();
                    cmd.CommandTimeout = 1200;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
