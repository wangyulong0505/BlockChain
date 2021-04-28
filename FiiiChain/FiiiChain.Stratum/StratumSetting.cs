using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Stratum
{

    public class MethodName
    {
        public const string Authorize_Name = "mining.authorize";
        public const string Subscribe_Name = "mining.subscribe";
        public const string Notify_Name = "mining.notify";
        public const string Submit_Name = "mining.submit";
        public const string Diffculty_Name = "mining.diffculty";
    }

    public enum MethodId
    {
        Authorize = 1,
        Subscribe,
        Notify,
        Submit,
        Diffculty
    }

    public class ResultErrorCode
    {
        #region ERROR
        public const int UNKNOWN_ERROR = 20; //其他/未知
        public const int NOTFOUND_ERROR = 21; //找不到工作（=陈旧）
        public const int REPEATSHARE_ERROR = 22; //重复份额
        public const int LOWDIFFICULTY_ERROR = 23; //低难度份额
        public const int UNAUTHORIZED_ERROR = 24; //未经授权
        public const int UNSUBSCRIBER_ERROR = 25; //未订阅
        #endregion

        public const int PARAMS_ERROR = 1002; //参数不符合要求
        public const int AUTHORIZED_ERROR = 2000; //授权失败
        public const int AUTHORIZED_REPEAT_ERROR = 2001; //重复授权

        public const int BLOCK_HEIGHT_ERROR = 3001; //区块高度不符合要求
        public const int BLOCK_SCOOPNUMBER_ERROR = 3002; //区块高度不符合要求
        public const int BLOCK_EFFORTMAXNONCE_ERROR = 3003; //工作量等于最大nonce
        public const int BLOCK_TASKNOTFOUND_ERROR = 3004; //当前任务不存在
        public const int BLOCK_VERIFY_ERROR = 3005; //当前任务不存在

        public const int SUBMIT_STOP_ERROR = 4001; //提交暂停


    }
}
