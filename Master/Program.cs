using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using NetX.Common;
using NetX.MemoryQueue;

namespace NetX.Master
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseLogging();
            builder.WebHost.AddGrpcHost(Convert.ToInt32(builder.Configuration["Master:Port"]));
            builder.Services.AddMaster();
            var app = builder.Build();
            app.UseMaster();
            app.Run();
        }
    }
}