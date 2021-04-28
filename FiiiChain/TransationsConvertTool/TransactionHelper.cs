// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TransationsConvertTool
{
    public class TransactionHelper
    {
        public static List<T> GetData<T>(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            List<T> result = new List<T>();

            var files = Directory.GetFiles(dir);

            foreach (var file in files)
            {
                using (var stream = File.Open(file, FileMode.Open))
                {
                    try
                    {
                        BinaryFormatter b = new BinaryFormatter();
                        var obj = b.Deserialize(stream);

                        if (obj != null)
                        {
                            result.Add((T)obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Tx File Deserialize failed.");
                        //Console.WriteLine("Tx File Deserialize failed:" + file, ex);
                    }
                }
            }
            return result;
        }
    }
}