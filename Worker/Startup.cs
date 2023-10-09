namespace NetX.Worker;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddWorker(_configuration);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWorker();
    }
}
