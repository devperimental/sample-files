namespace Helper.Startup.Types
{
    public class BootstrapConfiguration
    {
        public string? EnvironmentName { get; set; }
        public string? AspNetCoreEnvironmentName { get; internal set; }
        public string? ServiceName { get; internal set; }
        public string? AwsAccessKey { get; internal set; }
        public string? AwsAccessSecret { get; internal set; }
    }
}
