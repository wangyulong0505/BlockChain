using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.InvestorAward
{
    public class AppSetting
    {
        public List<TimeSettings> TimeSetting { get; set; }

        public string ChangeAddress { get; set; }

        public long FeeRate { get; set; }
    }

    public class TimeSettings
    {
        public string Time { get; set; }
    }
}
