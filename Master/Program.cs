using NetX.Common;

namespace NetX.Master;

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
                        var configuration = new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                        .Build();
                        var configPort = configuration.GetSection("Master:Port").Value;
                        int port = 5600;
                        int.TryParse(configPort, out port);
                        options.ListenAnyIP(port, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    });
                    webHostBuilder.UseUrls($"http://*".AddRandomPort());
                    webHostBuilder.UseStartup<Startup>();
                });
        hostBuilder.UseLogging();
        var host = hostBuilder.Build();
        await host.RunAsync();
    }
}