using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.Stratum.PoolSocket
{
    public class ClientUser
    {
        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public string IPAddress { get; set; }

        /// <summary>
        /// 是否已授权
        /// </summary>
        public bool Authorized { get; set; }
        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool Subscribed { get; set; }

        /// <summary>  
        /// 客户端地址  
        /// </summary>  
        public EndPoint Remote { get; set; }

        /// <summary>  
        /// 通信Socket 
        /// </summary>  
        public Socket Socket { get; set; }

        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }

        /// <summary>  
        /// 有效时间  
        /// </summary>  
        public DateTime ActiveDateTime { get; set; }
        /// <summary>
        /// 矿机地址
        /// </summary>
        public string WalletAddress { get; set; }
        /// <summary>
        /// 矿机序列号
        /// </summary>
        public string SerialNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CheckScoopNumber { get; set; }
        /// <summary>  
        /// 数据缓存区  
        /// </summary>  
        public List<byte> Buffer { get; set; }

        public ClientUser()
        {
            this.Buffer = new List<byte>();
        }

    }

    public class ClientUserList
    {
        private List<ClientUser> m_list;

        public ClientUserList()
        {
            m_list = new List<ClientUser>();
        }

        public void Add(ClientUser user)
        {
            lock (m_list)
            {
                m_list.Add(user);
            }
        }

        public void Remove(ClientUser user)
        {
            lock (m_list)
            {
                m_list.Remove(user);
            }
        }

        public void CopyList(ref ClientUser[] array)
        {
            lock (m_list)
            {
                array = new ClientUser[m_list.Count];
                m_list.CopyTo(array);
            }
        }
    }
}
