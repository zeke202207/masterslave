using Hangfire;
using HangfireBasicAuthenticationFilter;

namespace NetX.Master;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMaster(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // 配置Hangfire Dashboard路径和权限控制
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AppPath = null,
            DashboardTitle = "ms jobs",
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = configuration.GetSection("HangfireCredentials:UserId").Value,
                    Pass = configuration.GetSection("HangfireCredentials:Password").Value
                }
            }
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<MasterService>();
            endpoints.MapGrpcService<ServiceSDK>();
            endpoints.MapGrpcService<MonitorSDK>();
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }
}
