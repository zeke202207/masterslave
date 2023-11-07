using SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class ServiceLoginMiddleware : IApplicationMiddleware<GrpcContext<ServiceLoginRequest, ServiceLoginResponse>>
{
    private readonly IJwtManager _jwtManager;

    public ServiceLoginMiddleware(IJwtManager jwtManager)
    {
        _jwtManager = jwtManager;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<ServiceLoginRequest, ServiceLoginResponse>> next, GrpcContext<ServiceLoginRequest, ServiceLoginResponse> context)
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
