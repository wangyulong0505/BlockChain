using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.Business;
using FiiiChain.DTO;
using FiiiChain.Entities;
using FiiiChain.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace FiiiChain.Wallet.Api.Test
{
    [TestClass]
    public class ApiTest
    {
        [TestMethod]
        public async System.Threading.Tasks.Task TestMethod1Async()
        {
            try
            {
                for (int i = 0; i < 200000; i++)
                {
                    AuthenticationHeaderValue auth = null;
                    RpcClient rpcClient = new RpcClient(new System.Uri("http://127.0.0.1:5006/"), auth, null, null, "application/json");
                    RpcRequest request = RpcRequest.WithParameterList("GetNewAddress", new List<object> { Guid.NewGuid()}, 1);
                    RpcResponse response = await rpcClient.SendRequestAsync(request);
                    if (response.HasError)
                    {
                        throw new Exception(response.Error.Message.ToString());
                    }
                    Account account = response.GetResult<Account>();
                    LogHelper.Info($"new address is {account.Id}");
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        
    }
}
