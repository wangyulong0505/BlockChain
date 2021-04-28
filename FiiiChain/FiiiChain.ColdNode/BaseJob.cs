using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.ColdNode
{
    public abstract class BaseJob
    {
        public abstract void Start();
        public abstract void Stop();
        public abstract JobStatus Status { get; }
    }

    public enum JobStatus
    {
        Stopped = 1,
        Running = 2,
        Stopping = 3,
    }
}
