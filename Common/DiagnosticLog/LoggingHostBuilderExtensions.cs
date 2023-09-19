using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace NetX.Common
{
    public static class LoggingHostBuilderExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder builder)
        {
            builder.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                var buildConfig = new ConfigurationBuilder()
                                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging.json"))
                                .Build();
                loggerConfiguration.ReadFrom.Configuration(buildConfig);
                loggerConfiguration.Enrich.FromLogContext()
                .WriteTo.Console();
            });
            return builder;
        }
    }
}
