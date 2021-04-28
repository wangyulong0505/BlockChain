// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Data;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TransationsConvertTool
{
    public class TxDac : DataAccessComponent<TxDac>
    {
        public bool HasTransaction(string transactionId)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Transactions " +
                "WHERE hash = @Hash " +
                " LIMIT 1 ";

            bool hasTransaction = false;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", transactionId);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    hasTransaction = dr.HasRows;
                }
            }

            return hasTransaction;
        }

        public bool HasUtxo(string transactionHash, int index)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM OutputList " +
                "WHERE IsDiscarded = 0 " +
                "AND TransactionHash = @Id and [index] = @Index LIMIT 1 ";

            bool hasTransaction = false;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", transactionHash);
                cmd.Parameters.AddWithValue("@Index", index);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    hasTransaction = dr.HasRows;
                }
            }

            return hasTransaction;
        }

        public bool HasCost(string transactionHash, int index)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM InputList " +
                "WHERE  " +
                "OutputTransactionHash = @Id and [OutputIndex] = @Index LIMIT 1 ";

            bool hasTransaction = false;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", transactionHash);
                cmd.Parameters.AddWithValue("@Index", index);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    hasTransaction = dr.HasRows;
                }
            }

            return hasTransaction;
        }
    }
}
