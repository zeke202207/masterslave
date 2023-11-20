using SDK;

namespace NetX.Master;

public class LoginMiddleware : IApplicationMiddleware<GrpcContext<LoginRequest, LoginResponse>>
{
    private readonly IJwtManager _jwtManager;

    public LoginMiddleware(IJwtManager jwtManager)
    {
        _jwtManager = jwtManager;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<LoginRequest, LoginResponse>> next, GrpcContext<LoginRequest, LoginResponse> context)
    {
        try
        {
            //TODO: username & password验证，给出唯一userid->根据userid和pwd生成token票据
            context.Response.Response.Token = _jwtManager.GenerateJwtToken(new JwtModel() { UserId = context.Reqeust.Request.UserName, Password = context.Reqeust.Request.Password });
            context.Response.Response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            context.Response.Response.IsSuccess = false;
            context.Response.Response.ErrorMessage = ex.Message;
        }
        await Task.CompletedTask;
    }
}
