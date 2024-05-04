using Serilog;

namespace WebServerWithMultipleVirtualHosts
{
    public class VirtualHost
    {
        public required string Name { get; set; }
        public required string Port { get; set; }
        public required string RootDir { get; set; }
        public required string LogFile { get; set; }
        public required ILogger Loger { get; set; }
    }
}
