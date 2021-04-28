using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Stratum
{

    /// <summary>
    /// 接收
    /// </summary>
    public class DataModel
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public object[] Params { get; set; }
        public object[] Result { get; set; }
        public int Error { get; set; }
    }
    /// <summary>
    /// 发送
    /// </summary>
    public class SendModel
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public object[] Params { get; set; }
    }

    /// <summary>
    /// 回复
    /// </summary>
    public class ReplyModel
    {
        public int Id { get; set; }
        public object[] Result { get; set; }
        public int Error{ get; set; }
    }


}
