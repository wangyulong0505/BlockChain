using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FiiiCoin
{
    public sealed class Core
    {
        public const string VERSION = "0.0.1";

        public static Action<string> InfoLogger { get; set; }
        public static Action<string> ErrorLogger { get; set; }

        static Core()
        {
#if DEBUG
            Debug.WriteLine("Initializing FiiiCoin version " + VERSION);
#endif
            InfoLogger.Invoke("Initializing FiiiCoin version " + VERSION);

            try
            {
                long startTime = Time.UnixTime;

                //Step 1: Check or upgrade Wallet version


                //Step 2: Initialize Transaction Processor

                                                           
                //Step 3: Initialize Blockchain Processor


                //Step 4: Initialize Account


                //Step 5: Initialize Peers Discovery


                //Step 6: Initialize Block Generator


                //Step 7: Initialize API
                
            }
            catch(Exception ex)
            {
                ErrorLogger.Invoke(ex.Message);
            }
        }
    }
}
