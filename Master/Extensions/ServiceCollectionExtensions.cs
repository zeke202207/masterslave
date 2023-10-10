﻿using NetX.MemoryQueue;

namespace NetX.Master;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaster(this IServiceCollection services)
    {
        services.AddSingleton<ResultDispatcher>();
        services.AddSingleton<INodeManagement, NodeManagement>();
        services.AddSingleton<ILoadBalancing, LruLoadBalancing>();
        services.AddSingleton<IJobPublisher, JobPublisher>();
        services.AddSingleton<IJobExecutor, JobExecutor>();
        services.AddSingleton<ISecurityPolicy, IpWhitelistSecurityPolicy>();
        services.AddGrpc();
        services.AddMemoryQueue(p => p.AsSingleton(), typeof(JobConsumer));
        return services;
    }
}
