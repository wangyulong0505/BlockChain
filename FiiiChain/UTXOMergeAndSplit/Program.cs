using FiiiChain.Framework;
using System;
using System.Threading.Tasks;

namespace UTXOMergeAndSplit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                MergeUTXO merge = new MergeUTXO();
                switch (args.Length)
                {
                    case 0:
                        await merge.Merge(false, null);
                        break;
                    case 1:
                        if (args[0].ToLower() == "-testnet")
                        {
                            await merge.Merge(true, null);
                        }
                        else
                        {
                            await merge.Merge(false, args[0]);
                        }
                        break;
                    case 2:
                        await merge.Merge(true, args[1]);
                        break;
                }
                /*
                SplitUTXO split = new SplitUTXO();
                switch (args.Length)
                {
                    case 0:
                        await split.Split(false, null);
                        break;
                    case 1:
                        if (args[0].ToLower() == "-testnet")
                        {
                            await split.Split(true, null);
                        }
                        else
                        {
                            await split.Split(false, args[0]);
                        }
                        break;
                    case 2:
                        await split.Split(true, args[1]);
                        break;
                }
                */
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }
    }
}
