namespace NetX.Master;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMaster();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMaster();

    }
}
