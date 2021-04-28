using FiiiChain.ShareModels.Msgs;
using Newtonsoft.Json;
using System;

namespace JsonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = "{\"BaseTarget\":537290144,\"Id\":\"f904bf10 - d09f - 4dba - 901b - b0be7c18188e\",\"BlockHeight\":20411,\"ScoopNumber\":811,\"StartTime\":1541487588504,\"GenHash\":[64,216,91,123,221,70,136,241,88,238,201,242,192,160,85,65,94,32,104,144,29,76,25,148,7,13,217,192,152,29,35,239]}";

            var msg = JsonConvert.DeserializeObject<StartMiningMsg>(json);
        }
    }
}
