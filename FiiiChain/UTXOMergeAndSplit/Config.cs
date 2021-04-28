using System;
using System.Collections.Generic;
using System.Text;

namespace UTXOMergeAndSplit
{
    /// <summary>
    /// 合并配置
    /// </summary>
    public class MergeConfig
    {
        /// <summary>
        /// 最终合并金额
        /// </summary>
        public long FinalAmount { get; set; }

        /// <summary>
        /// 参与合并的金额上限
        /// </summary>
        public long SearchMaxAmount { get; set; }

        /// <summary>
        /// 参与合并的金额下限
        /// </summary>
        public long SearchMinAmount { get; set; }

        /// <summary>
        /// 合并后金额输出地址
        /// </summary>
        public string SendAddress { get; set; }

        /// <summary>
        /// 找零地址
        /// </summary>
        public string ChangeAddress { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public long FeeRate { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 发送交易后休息时间，单位毫秒
        /// </summary>
        public int SleepSeconds { get; set; }

        /// <summary>
        /// 交易数目限制，大于等于交易数目会中断
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// 拆分配置
    /// </summary>
    public class SplitConfig
    {
        /// <summary>
        /// 最终输出金额
        /// </summary>
        public long FinalAmount { get; set; }

        /// <summary>
        /// 参与拆分的金额上限
        /// </summary>
        public long SplitMaxAmount { get; set; }

        /// <summary>
        /// 参与拆分的金额下限
        /// </summary>
        public long SplitMinAmount { get; set; }

        /// <summary>
        /// 金额输出地址
        /// </summary>
        public string SendAddress { get; set; }

        /// <summary>
        /// 找零地址
        /// </summary>
        public string ChangeAddress { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public long FeeRate { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
    }
}
