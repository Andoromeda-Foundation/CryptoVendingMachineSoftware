namespace XiaoTianQuanServer.Settings
{
    public class DefaultConfiguration
    {
        public int MachineLockTimeout { get; set; } = 180;
        public int PaymentTimeout { get; set; } = 100;
        public int DisplayTimeout { get; set; } = 60;
        public int ReleaseProductTimeout { get; set; } = 60;
    }
}