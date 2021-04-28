// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiiiChain.PosPool.Models
{
    public class SafeList<T> : System.Collections.Concurrent.ConcurrentBag<T> where T : class
    {
        public void Remove(T item)
        {
            var newItmes = this.Except(new List<T> { item }).ToList();
            this.Clear();
            newItmes.ToList().ForEach(x => this.Add(x));
        }
    }
}
