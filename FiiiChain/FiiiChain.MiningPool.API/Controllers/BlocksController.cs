using FiiiChain.Consensus.Api;
using FiiiChain.Framework;
using FiiiChain.MiningPool.API.Config;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.API.Controllers
{
    [Obsolete]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlocksController : BaseController
    {
        IHostingEnvironment env;
        public BlocksController(IHostingEnvironment _env)
        {
            env = _env;
        }
        /// <summary>
        /// 保存区块和奖励
        /// </summary>
        /// <param name="blockSaved"></param>
        /// <returns></returns>
        [HttpPost]
        public CommonResponse SaveBlocks([FromBody]BlockSaved blockSaved)
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                component.SaveBlockAndRewardLists(blockSaved.block, blockSaved.RewardLists);
                return OK();
            }
            catch (ApiCustomException ce)
            {
                LogHelper.Error(ce.Message);
                return Error(ce.ErrorCode, ce.ErrorMessage);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 自动同步区块验证状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<CommonResponse> GetVerifiedHashes()
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                await component.GetVerifiedHashes();
                return OK();
            }
            catch (ApiCustomException ce)
            {
                LogHelper.Error(ce.Message);
                return Error(ce.ErrorCode, ce.ErrorMessage);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return Error(ex.HResult, ex.Message);
            }
        }

        [HttpGet]
        public CommonResponse Test()
        {

            ConfigurationTool tool = new ConfigurationTool();
            ServerSetting setting = tool.GetAppSettings<ServerSetting>("ServerSetting");

            if (setting.IsTestNet)
            {
                return OK("Test");
            }
            else
            {
                return OK("Main");
            }
        }
    }
}
