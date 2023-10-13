using Microsoft.Extensions.Configuration;
using NetX.Common;
using ServiceSelf;

namespace NetX.Master;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (Service.UseServiceSelf(args))
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                // ÎªHostÅäÖÃUseServiceSelf()
                .UseServiceSelf()
                .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging.json"));
                        config.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: true, reloadOnChange: true);
                    })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenAnyIP(6583.AddRandomPort(), listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
                        var configuration = new ConfigurationBuilder()
                                        .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: true, reloadOnChange: true)
                                        .Build();
                        options.ListenAnyIP(configuration.GetValue<int>("Master:Port"), listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    });
                    webHostBuilder.UseStartup<Startup>();
                });
            hostBuilder.UseLogging();
            var host = hostBuilder.Build();
            await host.RunAsync();
        }
    }
}