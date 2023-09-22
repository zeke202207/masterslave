namespace NetX.Worker
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWorker(this IApplicationBuilder app)
        {
            app.UseRouting();
            return app;
        }
    }
}
