using FiiiChain.DataAgent;
using FiiiChain.Messages;
using Newtonsoft.Json;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TransationsConvertTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = null;
            try
            {
                dir = args[0];
                if (!Directory.Exists(dir))
                {
                    Console.WriteLine($"Directory {dir} not found");
                    return;
                }
            }
            catch
            {
                Console.WriteLine($"dotnet TransationsConvertTool [dir]");
                return;
            }
            List<TransactionMsg> items = new List<TransactionMsg>();

            try
            {
                items = TransactionHelper.GetData<TransactionMsg>(dir);
            }
            catch
            {
                items = TransactionHelper.GetData<TransactionPoolItem>(dir).Select(x => x.Transaction).ToList();
            }
            ConvertToJson(items);

            CheckTxItem(items);

            Console.ReadKey();
        }

        static TxDac txDac = new TxDac();

        public static TxType Check(TransactionMsg tx)
        {
            var inputs = tx.Inputs;
            foreach (var input in inputs)
            {
                if (!txDac.HasTransaction(input.OutputTransactionHash))
                    return TxType.NoFoundUtxo;
                if (!txDac.HasUtxo(input.OutputTransactionHash, input.OutputIndex) || txDac.HasCost(input.OutputTransactionHash, input.OutputIndex))
                    return TxType.RepeatedCost;
            }
            return TxType.UnPackge;
        }

        public static void ConvertToJson<T>(T items)
        {
            var json = JsonConvert.SerializeObject(items);

            using (var fs = File.OpenWrite("trans.json"))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
            }
            Console.WriteLine("Convert to json sucesses");
        }

        static void CheckTxItem(List<TransactionMsg> items)
        {
            var sb = new StringBuilder();
            int RepeatedCost = 0;
            int NoFoundUtxo = 0;
            foreach (var item in items)
            {
                var result = Check(item);
                if (result != TxType.UnPackge)
                {
                    sb.AppendLine($"{result.ToString()}:{item.Hash}");
                    if (result == TxType.NoFoundUtxo)
                        NoFoundUtxo++;
                    else
                        RepeatedCost++;
                }
            }

            using (var fs = File.Create("result.txt"))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Write(bytes, 0, bytes.Length);
            }

            Console.WriteLine("RepeatedCostCount : " + RepeatedCost);
            Console.WriteLine("NoFoundUtxoCount : " + NoFoundUtxo);
            Console.WriteLine("UnPackgeCount : " + (items.Count - RepeatedCost - NoFoundUtxo));

            Console.WriteLine("CheckTxItem sucesses");
        }
    }

    public enum TxType
    {
        UnPackge,
        RepeatedCost,
        NoFoundUtxo
    }


    public class ZipHelper
    {
        public static byte[] Compress(byte[] inpbuf)
        {
            CoderPropID[] propIDs = {   CoderPropID.DictionarySize,
                                        CoderPropID.PosStateBits,
                                        CoderPropID.LitContextBits,
                                        CoderPropID.LitPosBits,
                                        CoderPropID.Algorithm,
                                        CoderPropID.NumFastBytes,
                                        CoderPropID.MatchFinder,
                                        CoderPropID.EndMarker
                                       };
            object[] properties = { (Int32)(23),
                                    (Int32)(2),
                                    (Int32)(3),
                                    (Int32)(2),
                                    (Int32)(1),
                                    (Int32)(128),
                                    (string)("bt4"),
                                    (bool)(true)
                               };
            var enc = new SevenZip.Compression.LZMA.Encoder();
            enc.SetCoderProperties(propIDs, properties);

            MemoryStream msInp = new MemoryStream(inpbuf);
            MemoryStream msOut = new MemoryStream();
            enc.WriteCoderProperties(msOut);
            enc.Code(msInp, msOut, -1, -1, null);
            return msOut.ToArray();
        }
        public static byte[] Decompress(byte[] inpbuf)
        {
            CoderPropID[] propIDs =
       {
    CoderPropID.DictionarySize,
    CoderPropID.PosStateBits,
    CoderPropID.LitContextBits,
    CoderPropID.LitPosBits,
    CoderPropID.Algorithm,
    CoderPropID.NumFastBytes,
    CoderPropID.MatchFinder,
    CoderPropID.EndMarker
   };
            object[] properties =
                                   {
                                (Int32)(23),
                                (Int32)(2),
                                (Int32)(3),
                                (Int32)(2),
                                (Int32)(1),
                                (Int32)(128),
                                (string)("bt4"),
                                (Int32)(0)
                               };
            var dec = new SevenZip.Compression.LZMA.Decoder();
            byte[] prop = new byte[5];
            Array.Copy(inpbuf, prop, 5);
            dec.SetDecoderProperties(prop);
            MemoryStream msInp = new MemoryStream(inpbuf);
            msInp.Seek(5, SeekOrigin.Current);
            MemoryStream msOut = new MemoryStream();
            dec.Code(msInp, msOut, -1, -1, null);
            return msOut.ToArray();
        }
    }

}
