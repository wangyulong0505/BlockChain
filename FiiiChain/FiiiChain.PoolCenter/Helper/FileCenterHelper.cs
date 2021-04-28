using FiiiChain.Messages;
using FiiiChain.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.PoolCenter.Helper
{
    public static class FileCenterHelper
    {
        /// <summary>
        /// 把区块信息写进文本文件中，主要供守护程序使用
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteBlockInfo(BlockMsg msg)
        {
            string filePath = "blockinfo.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write($"{msg.Header.Hash}|{msg.Header.Height}|{msg.Header.Timestamp}");
                writer.Flush();
                LogHelper.Info($"blockinfo time is {msg.Header.Timestamp}");
            }
        }
    }
}
