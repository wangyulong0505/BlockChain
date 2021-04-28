using System;

namespace MultiThreadingMiners
{
    class Program
    {
        static void Main(string[] args)
        {
            /* 1、调用GenerateNewAddress产生地址，然后读取外部文件，把对应的地址，SN， Account写进mysql数据库中
             * 2、每20-50个POS机一组，开一个单独的线程，循环调用Api
             * 3、开始挖矿，随机每隔30-60分钟调一次GetScoopNumber，SubmitMaxNonce（131072）
             * 4、随机每隔10-30分钟调一次GetPaidReward和GetUnPaidReward
             */
            //先从数据库中获取所有
        }
    }
}
