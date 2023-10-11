using Hangfire;
using HangfireBasicAuthenticationFilter;

namespace NetX.Master;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMaster(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseRouting();

        // 配置Hangfire Dashboard路径和权限控制
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AppPath = null,
            DashboardTitle = "Hangfire Dashboard Test",
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = configuration.GetSection("HangfireCredentials:UserName").Value,
                    Pass = configuration.GetSection("HangfireCredentials:Password").Value
                }
            }
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<MasterService>();
            endpoints.MapGrpcService<MasterServiceSDK>();
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }
}
