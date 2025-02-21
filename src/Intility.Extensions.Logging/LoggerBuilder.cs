using Microsoft.Extensions.Hosting;
using Serilog;

namespace Intility.Extensions.Logging
{
    public class LoggerBuilder(HostBuilderContext host, LoggerConfiguration config, IHostBuilder hostBuilder) : ILoggerBuilder
    {
        public ConsoleFormat ConsoleFormat { get; private set; } = ConsoleFormat.Pretty;

        public HostBuilderContext Host => host;
        public IHostBuilder HostBuilder => hostBuilder;
        public LoggerConfiguration Configuration => config;

        public ILoggerBuilder UseConsoleFormat(ConsoleFormat format)
        {
            ConsoleFormat = format;

            return this;
        }
    }
}
