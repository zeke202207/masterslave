using Microsoft.AspNetCore.Server.Kestrel.Core;
using NetX.Common;

namespace NetX.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("logging.json");
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenAnyIP(7561.AddRandomPort(), listenOptions => listenOptions.Protocols = HttpProtocols.Http1);

                    });
                    //webHostBuilder.UseUrls($"http://*".AddRandomPort());
                    webHostBuilder.UseStartup<Startup>();
                });
        hostBuilder.UseLogging();
        var host = hostBuilder.Build();
        await host.RunAsync();
    }
}