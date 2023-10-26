using System.Runtime.Versioning;

namespace NetX.Monitor
{
    internal class Program
    {
        private static async Task Main()
        {
            if (OperatingSystem.IsWindows())
                SetWindowSize();
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json")
               .AddEnvironmentVariables();
            var configuration = builder.Build();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Services
                    services.AddSingleton<CommunicationService>();
                    services.AddTransient<TerminalOrchestrator>();

                    // Windows
                    services.AddTransient<MainWindow>();
                    services.AddTransient<LoginWindow>();

                    services.AddLogging(builder =>
                    {
                        builder
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("NToastNotify", LogLevel.Warning)
                            .AddConsole();
                    }
                    );
                })
                .Build();

            var svc = ActivatorUtilities.CreateInstance<TerminalOrchestrator>(host.Services);
            await svc.Run();
        }

        /// <summary>
        /// 调整控制台大小
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static void SetWindowSize()
        {
            Console.SetWindowSize(140, 40);
        }
    }
}