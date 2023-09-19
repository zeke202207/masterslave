using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace NetX.Master
{
    public static class WebApplicationBuilderExtentions
    {
        public static IWebHostBuilder AddGrpcHost(this IWebHostBuilder hostBuilder, int port)
        {
            hostBuilder.ConfigureKestrel(
                options =>
                {
                    options.ListenAnyIP(port, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                });
            return hostBuilder;
        }
    }
}
