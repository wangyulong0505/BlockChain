using FiiiChain.Business;
using FiiiChain.Framework;
using System;
using System.Threading;

namespace FiiiChain.ColdNode
{
    public class BlockJob : BaseJob
    {
        bool isRunning = false;
        BlockComponent blockComponent;
        Thread thread;

        public BlockJob()
        {
            blockComponent = new BlockComponent();
            blockComponent.BlockPoolInitialize();
        }

        public override JobStatus Status
        {
            get
            {
                if (thread == null || (thread.ThreadState != ThreadState.Running && thread.ThreadState != ThreadState.WaitSleepJoin))
                {
                    return JobStatus.Stopped;
                }
                else if ((thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.WaitSleepJoin) && !isRunning)
                {
                    return JobStatus.Stopping;
                }
                else
                {
                    return JobStatus.Running;
                }
            }
        }

        public override void Start()
        {
            this.isRunning = true;
            this.thread = new Thread(new ThreadStart(this.verifyBlocksInDB));
            //this.thread.IsBackground = true;
            this.thread.Start();
        }

        public override void Stop()
        {
            this.isRunning = false;
            //this.thread.Abort();
        }

        public long GetLatestHeight()
        {
            return 0;
        }

        private void verifyBlocksInDB()
        {
            while (this.isRunning)
            {
                try
                {
                    this.blockComponent.ProcessUncsonfirmedBlocks();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.ToString());
                }
                //this.blockComponent.SaveVerifiedBlockIntoDB();
                Thread.Sleep(30000);
            }
        }
    }
}
