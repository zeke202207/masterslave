using Hangfire;
using Hangfire.MemoryStorage;
using NetX.MemoryQueue;

namespace NetX.Master;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaster(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddControllers();

        // 注册Hangfire服务
        // 注册Hangfire服务器
        services.AddHangfireServer(option =>
                                        {
                                            option.ServerCheckInterval = TimeSpan.FromSeconds(1);
                                            option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
                                            option.WorkerCount = 1;
                                            option.CancellationCheckInterval = TimeSpan.FromSeconds(1);
                                            option.HeartbeatInterval = TimeSpan.FromSeconds(1);
                                            option.ServerTimeout = TimeSpan.FromSeconds(5);
                                            option.ServerName = "hangfire-server";
                                        })
            .AddHangfire(config => config.UseMemoryStorage());

        services.AddTransient<IJob, CleanupResultConsumer>();
        services.AddTransient<IJob, CleanupWorkerNode>();
        services.AddHostedService<HangFireHostService>();

        services.AddSingleton<IResultDispatcher, ResultDispatcher>();
        services.AddSingleton<INodeManagement, NodeManagement>();
        services.AddSingleton<ILoadBalancing, LruLoadBalancing>();
        services.AddSingleton<IJobPublisher, JobPublisher>();
        services.AddSingleton<IJobExecutor, JobExecutor>();
        services.AddSingleton<ISecurityPolicy, IpWhitelistSecurityPolicy>();
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcConnectionInterceptor>();
            options.MaxSendMessageSize = int.MaxValue;
            options.MaxReceiveMessageSize = int.MaxValue;
        });
        services.AddMemoryQueue(p => p.AsSingleton(), typeof(JobConsumer));

        return services;
    }
}
