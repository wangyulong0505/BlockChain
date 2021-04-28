namespace FiiiChain.RabbitMQ
{
    public class RabbitMqSetting
    {
        public static readonly string HOSTNAME = "localhost";
        public static readonly string USERNAME = "king";
        public static readonly string PASSWORD = "880505";
        public static readonly string EXCHANGENAME = "direct";

        //host=myServer;virtualHost=myVirtualHost;username=mike;password=topsecret
        //测试环境地址
        public static readonly string TESTCONNECTIONSTRING = "amqp://fiiipay:fp123456%40@172.31.126.73:5672/fiiipay";
        //生产环境地址 
        //public static readonly string MAINCONNECTIONSTRING = "amqp://fiiipay:fp123456%40@172.31.126.42:5672/fiiipay";
        //本地地址："amqp://user:pass@hostName:port/vhost";
        //public static readonly string CONNECTIONSTRING = "amqp://king:880505@localhost:5672/king";
        //public static readonly string LOCALCONNECTIONSTRING = "amqp://king:880505@192.168.1.128:5672/king";
    }

    public class RabbitMqName
    {
        public const string StartMining = "StartMining";
        public const string StopMining = "StopMining";
        public const string Login = "Login";
        public const string ForgetBlock = "ForgetBlock";
        public const string HeartPool = "HeartPool";
        public const string Default = "Default";
        public const string FiiiPosInviteReward = "FiiiPosInviteReward";
    }
}
