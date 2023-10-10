namespace NetX.Worker;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWorker(this IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }
}
