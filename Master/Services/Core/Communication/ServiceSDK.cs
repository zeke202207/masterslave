using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SDK;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的SDK
/// </summary>
public class ServiceSDK : SDK.MasterServiceSDK.MasterServiceSDKBase
{
    private readonly IServiceProvider _appServices;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="logger"></param>
    /// <param name="dataTransferCenter"></param>
    public ServiceSDK(IServiceProvider serviceProvider)
    {
        _appServices = serviceProvider;
    }

    public override async Task<ServiceLoginResponse> Login(ServiceLoginRequest request, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new ServiceLoginResponse());

        var application = new ApplicationBuilder<GrpcContext<ServiceLoginRequest, ServiceLoginResponse>>(_appServices)
                .Use<ExceptionMiddleware<ServiceLoginRequest, ServiceLoginResponse>>()
                .Use<ServiceLoginMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        return grpcContext.Response.Response;
    }

    /// <summary>
    /// 开始执行任务
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Authorize]
    public override async Task ExecuteTask(ExecuteTaskRequest request, IServerStreamWriter<ExecuteTaskResponse> responseStream, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new ExecuteTaskResponse());
        grpcContext.Response.ResponseStream = responseStream;

        var application = new ApplicationBuilder<GrpcContext<ExecuteTaskRequest, ExecuteTaskResponse>>(_appServices)
                .Use<ExceptionMiddleware<ExecuteTaskRequest, ExecuteTaskResponse>>()
                .Use<ExecuteTaskMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        await Task.FromResult(grpcContext.Response.Response);
    }
}
