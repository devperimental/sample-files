namespace Helper.Startup.Types
{
    public class BootstrapConfiguration
    {
        public string? CorsOrigins { get; set; }
        public string? EnvironmentName { get; set; }
        public bool AddHealthCheck { get; set; }
        public string? AspNetCoreEnvironmentName { get; internal set; }
        public string? ServiceName { get; internal set; }
    }
}
