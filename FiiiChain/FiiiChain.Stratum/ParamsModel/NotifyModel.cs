using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Stratum.ParamsModel
{
    public class NotifyModel
    {
        public Guid BF { get; set; }
        public int BlockHeight { get; set; }
        public int ScoopNumber { get; set; }
        public long StartTime { get; set; }
        public int GenHash { get; set; }
        /// <summary>
        /// 任务状态true=表示其他矿工已爆块
        /// </summary>
        public int Status { get; set; }
    }
}
