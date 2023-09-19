using Microsoft.Extensions.Configuration;
using NetX.MemoryQueue;

namespace NetX.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddWorker(builder.Configuration);
            var app = builder.Build();
            app.UseWorker();
            app.Run();
        }
    }
}