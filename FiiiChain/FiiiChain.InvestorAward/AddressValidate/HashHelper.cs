﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using System.Security.Cryptography;

namespace FiiiChain.InvestorAward.AddressValidate
{
    public class HashHelper
    {
        public static byte[] Hash(byte[] bytes)
        {
            SHA256 sha256 = SHA256Managed.Create();
            return sha256.ComputeHash(bytes);
        }

        public static byte[] DoubleHash(byte[] bytes)
        {
            return Hash(
                    Hash(bytes)
                );
        }
        public static byte[] EmptyHash()
        {
            return new byte[32];
        }
    }
}
