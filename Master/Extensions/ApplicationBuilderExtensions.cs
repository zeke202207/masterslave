namespace NetX.Master
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMaster(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<MasterService>();
                endpoints.MapGrpcService<MasterServiceSDK>();
            });
            return app;
        }
    }
}
