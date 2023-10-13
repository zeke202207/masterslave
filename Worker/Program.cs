using Microsoft.AspNetCore.Server.Kestrel.Core;
using NetX.Common;
using ServiceSelf;

namespace NetX.Worker;

public class Program
{
    public static void Main(string[] args)
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
                        options.ListenAnyIP(7561.AddRandomPort(), listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
                    });
                    webHostBuilder.UseStartup<Startup>();
                });
            hostBuilder.UseLogging();
            var host = hostBuilder.Build();
            host.Run();
        }
    }
}