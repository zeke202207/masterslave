using Grpc.Net.Compression;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetX.Master.Services.Core;
using NetX.MemoryQueue;
using System.Text;

namespace NetX.Master;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaster(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddControllers();

        // 注册Hangfire服务
        services.AddHangfireServer(option =>
                                        {
                                            option.ServerCheckInterval = TimeSpan.FromSeconds(1);
                                            option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
                                            option.WorkerCount = 1;
                                            option.CancellationCheckInterval = TimeSpan.FromSeconds(1);
                                            option.HeartbeatInterval = TimeSpan.FromSeconds(1);
                                            option.ServerTimeout = TimeSpan.FromSeconds(5);
                                            option.ServerName = "master-server";
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
        services.AddTransient<IJwtManager, JwtManager>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcConnectionInterceptor>();
            options.MaxSendMessageSize = int.MaxValue;
            options.MaxReceiveMessageSize = int.MaxValue;
            options.EnableDetailedErrors = true;
            //根据业务需求，可以选择开启grpc压缩（未来可配置到appsettings.json配置文件中）
            //options.ResponseCompressionAlgorithm = "gzip";
            //options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Fastest;
            //options.CompressionProviders.Add(new GzipCompressionProvider(System.IO.Compression.CompressionLevel.Fastest));
        });
        services.AddMemoryQueue(p => p.AsSingleton(), typeof(JobConsumer));

        //注入monitor tracker cache
        services.AddSingleton<IJobTrackerCache<JobTrackerItem>>(provider =>
        {
            // 这里可以根据需要传入适当的参数
            int capacity = 100;
            return new JobTrackerCache<JobTrackerItem>(capacity);
        });
        return services;
    }
}
