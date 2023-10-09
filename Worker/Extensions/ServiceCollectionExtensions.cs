using NetX.WorkerPlugin.Contract;
using System.Reflection;
using System.Runtime.Loader;

namespace NetX.Worker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<WorkerConfig>(config.GetSection("WorkerConfig"));
        services.AddSingleton<IMasterClient, MasterClient>();
        services.AddHostedService<WorkerHostedService>();
        services.AddPlugin();
        return services;
    }

    /// <summary>
    /// 注入项点处理方法
    /// </summary>
    /// <param name="services">ico服务集合</param>
    /// <returns></returns>
    public static IServiceCollection AddPlugin(this IServiceCollection services)
    {
        var pluginPath = Path.Combine(AppContext.BaseDirectory, "Plugin");
        if (!Directory.Exists(pluginPath))
            Directory.CreateDirectory(pluginPath);
        foreach (var file in Directory.EnumerateFiles(pluginPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                ////约定大于配置，所有项点动态库均需要以 XXXX 开头
                //if (!Path.GetFileNameWithoutExtension(file).StartsWith("XXXX"))
                //    continue;
                services.AddServicesFromAssembly(Assembly.Load(File.ReadAllBytes(file)));
            }
            catch (Exception ex)
            {
                //加载项点动态库失败
                Console.WriteLine($"加载插件动态库失败:{ex.ToString()}");
            }
        }
        return services;
    }

    /// <summary>
    /// 从指定程序集中注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            #region ==单例注入==

            var singletonAttr = (SingletonAttribute)Attribute.GetCustomAttribute(type, typeof(SingletonAttribute));
            if (singletonAttr != null)
            {
                //注入自身类型
                if (singletonAttr.Itself)
                {
                    services.AddSingleton(type);
                    continue;
                }

                var interfaces = type.GetInterfaces().Where(m => m != typeof(IDisposable)).ToList();
                if (interfaces.Any())
                {
                    foreach (var i in interfaces)
                    {
                        services.AddSingleton(i, type);
                    }
                }
                else
                {
                    services.AddSingleton(type);
                }

                continue;
            }

            #endregion

            #region ==瞬时注入==

            var transientAttr = (TransientAttribute)Attribute.GetCustomAttribute(type, typeof(TransientAttribute));
            if (transientAttr != null)
            {
                //注入自身类型
                if (transientAttr.Itself)
                {
                    services.AddSingleton(type);
                    continue;
                }

                var interfaces = type.GetInterfaces().Where(m => m != typeof(IDisposable)).ToList();
                if (interfaces.Any())
                {
                    foreach (var i in interfaces)
                    {
                        services.AddTransient(i, type);
                    }
                }
                else
                {
                    services.AddTransient(type);
                }
                continue;
            }

            #endregion

            #region ==Scoped注入==
            var scopedAttr = (ScopedAttribute)Attribute.GetCustomAttribute(type, typeof(ScopedAttribute));
            if (scopedAttr != null)
            {
                //注入自身类型
                if (scopedAttr.Itself)
                {
                    services.AddSingleton(type);
                    continue;
                }

                var interfaces = type.GetInterfaces().Where(m => m != typeof(IDisposable)).ToList();
                if (interfaces.Any())
                {
                    foreach (var i in interfaces)
                    {
                        services.AddScoped(i, type);
                    }
                }
                else
                {
                    services.AddScoped(type);
                }
            }

            #endregion
        }

        return services;
    }
}
