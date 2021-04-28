using FiiiChain.Framework;
using FiiiChain.Stratum.ParamsModel;
using ServerSocket.Stratum.ParamsModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace FiiiChain.Stratum
{
    public class StratumHelper
    {
        /// <summary>
        /// 获取授权Model
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static AuthorizeModel GetAuthorizeModel(object[] obj)
        {
            AuthorizeModel authorize = null;
            try
            {
                if (obj != null && obj.Length == 3)
                {
                    authorize = new AuthorizeModel();
                    authorize.WalletAddress = GetParams<string>(obj[0]);
                    authorize.MinerType = GetParams<int>(obj[1]);
                    authorize.SerialNo = GetParams<string>(obj[2]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"GetAuthorizeModel is error:{ex.Message}");
            }
            return authorize;
        }
        public static SubmitModel GetSubmitModel(object[] obj)
        {
            SubmitModel submit = null;
            try
            {
                if (obj != null && obj.Length == 6)
                {
                    submit = new SubmitModel();
                    submit.BF = GetParams<string>(obj[0]);
                    submit.BlockHeight = GetParams<int>(obj[1]);
                    submit.WalletAddress = GetParams<string>(obj[2]);
                    submit.Nonce = GetParams<int>(obj[3]);
                    submit.ScoopNumber = GetParams<int>(obj[4]);
                    submit.Target = GetParams<byte[]>(obj[5]);
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.Error($"GetSubmitModel is error:{ex.Message}");
            }
            return submit;
        }


        private static T GetParams<T>(object obj)
        {
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                LogHelper.Error($"GetParams,obj={obj} error:{ex.Message}");
            }
            return default(T);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetCorrectByte(byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(data);

            byte[] rev = null;
            //判断包的长度,前面4个字节.
            byte[] lenBytes = buffer.GetRange(0, 4).ToArray();
            int packageLen = BitConverter.ToInt32(lenBytes, 0);
            if (packageLen <= buffer.Count - 4)
            {
                rev = buffer.GetRange(4, packageLen).ToArray();
                buffer.RemoveRange(0, packageLen + 4);

                string json = Encoding.UTF8.GetString(rev);
                return json;
            }
            return "";
        }

        /// <summary>
        /// 字节加上前缀和后缀传输
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static byte[] PackToByte(string json)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(json);

            byte[] buffer = new byte[sendBuffer.Length + 4];
            Array.Copy(BitConverter.GetBytes(sendBuffer.Length), buffer, 4);
            Array.Copy(sendBuffer, 0, buffer, 4, sendBuffer.Length);
            return buffer;
        }

        /// <summary>
        /// 实体互转
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T2 CopyToModel<T1, T2>(T1 source)
        {
            try
            {
                T2 model = default(T2);
                PropertyInfo[] pi = typeof(T2).GetProperties();
                PropertyInfo[] pi1 = typeof(T1).GetProperties();
                model = Activator.CreateInstance<T2>();
                for (int i = 0; i < pi.Length; i++)
                {
                    pi[i].SetValue(model, pi1[i].GetValue(source, null), null);
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return default(T2);
            }
        }
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static byte[] ObjectToByte(object obj)
        {
            try
            {
                string temp = JsonConvert.SerializeObject(obj);

                return PackToByte(temp);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return null;
            }
        }

        public static T ByteToObject<T>(byte[] bytes)
        {
            try
            {
                string temp = GetCorrectByte(bytes);

                return JsonConvert.DeserializeObject<T>(temp);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return default(T);
            }
        }
    }
}
